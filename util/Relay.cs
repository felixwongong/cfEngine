/**
 * ######################################################################################
 * #                   Relay: Fast, light, GC-friendly signals/events.                  #
 * #                     (c) 2017-2019 Luke Thompson / Sigtrap Games                    #
 * #    Provided under MIT License. No warranty, it's all your own fault, blah blah.    #
 * #                   @six_ways    @sigtrapgames    github.com/sixways                 #
 * ######################################################################################
 * 
 * ### Adding/removing listeners DURING DISPATCH: #######################################
 * #  Relay r;
 * #  r.AddListener(B);
 * #  A(){}
 * #  B(){
 * #    r.RemoveListener(B);     // [1] Fine
 * #    r.AddListener(A);        // [2] Fine
 * #    r.RemoveListener(A);     // [3] Bad
 * #    r.RemoveAll();           // [4] Bad
 * #  }
 * # 
 * #  [1] A listener can safely REMOVE ITSELF from the Relay calling it.
 * #  [2] A listener can safely ADD NEW LISTENERS to the Relay calling it.
 * #         > New listeners will NOT be called until the next Dispatch.
 * #  [3] A listener should NOT REMOVE OTHER LISTENERS from the Relay calling it.
 * #  [4] A listener should NOT REMOVE ALL LISTENERS from the Relay calling it.
 * ######################################################################################
 *
 * ### Dispatch execution order: ########################################################
 * #  RELYNG ON DISPATCH ORDER WITH ANY EVENT SYSTEM IS AN ANTI-PATTERN!
 * #  Dispatch will be LIFO (i.e. in reverse) if the above rules are followed.
 * #    This is to allow:
 * #      > listener self-removal during dispatch without iterator skipping next listener
 * #      > addition of new listeners during dispatch without firing new listener
 * ######################################################################################
 */

using System;

namespace cfEngine.Util {
	public abstract class RelayBase<TDelegate> : IRelayLinkBase<TDelegate> where TDelegate:class {
		/// <summary>
		/// How many persistent listeners does this intance currently have?
		/// </summary>
		public uint listenerCount {get {return _count;}}
		/// <summary>
		/// How many one-time listeners does this intance currently have?
		/// After dispatch, all current one-time listeners are automatically removed.
		/// </summary>
		public uint oneTimeListenersCount {get {return _onceCount;}}

		protected bool _hasLink = false;

		protected TDelegate[] _listeners = new TDelegate[1];
		protected uint _count = 0;
		protected uint _cap = 1;

		protected TDelegate[] _listenersOnce;
		protected uint _onceCount = 0;
		protected uint _onceCap = 0;

		protected static IndexOutOfRangeException _eIOOR = new IndexOutOfRangeException("Fewer listeners than expected. See guidelines in Relay.cs on using RemoveListener and RemoveAll within Relay listeners.");

		#if SIGTRAP_RELAY_DBG
		/// <summary>
		/// If true, RelayDebugger will automatically record all listener addition and removal on all Relays.
		/// This allows a dump of all Relay data to aid diagnosis of lapsed listeners etc.
		/// </summary>
		public static bool recordDebugData {
			get {return _RelayDebugger.recordDebugData;}
			set {_RelayDebugger.recordDebugData = value;}
		}
		/// <summary>
		/// Output a log of all existing Relays and their listeners.
		/// </summary>
		/// <returns>The listeners.</returns>
		public static string LogRelays(){
			return _RelayDebugger.LogRelays();
		}
		/// <summary>
		/// Output a log of any and all Relays specified object is currently subscribed to.
		/// </summary>
		/// <returns>The listeners.</returns>
		/// <param name="observer">Owner of listeners.</param>
		public static string LogRelays(object observer){
			return _RelayDebugger.LogRelays(observer);
		}
		#endif

		#region API
		/// <summary>
		/// Is this delegate already a persistent listener?
		/// Does NOT query one-time listeners.
		/// </summary>
		/// <param name="listener">Listener.</param>
		public bool Contains(TDelegate listener){
			return Contains(_listeners, _count, listener);
		}
		/// <summary>
		/// Adds a persistent listener.
		/// </summary>
		/// <returns><c>True</c> if successfully added listener, <c>false</c> otherwise</returns>
		/// <param name="listener">Listener.</param>
		/// <param name="allowDuplicates">If <c>false</c>, checks whether persistent listener is already present.</param>
		public bool AddListener(TDelegate listener, bool allowDuplicates=false){
			if (!allowDuplicates && Contains(listener)) return false;
			if (_count == _cap){
				_cap *= 2;
				_listeners = Expand(_listeners, _cap, _count);
			}
			_listeners[_count] = listener;
			++_count;
			#if SIGTRAP_RELAY_DBG
			_RelayDebugger.DebugAddListener(this, listener);
			#endif

			return true;
		}
		/// <summary>
		/// Adds listener and creates a RelayBinding between the listener and the Relay.
		/// The RelayBinding can be used to enable/disable the listener.
		/// </summary>
		/// <returns>A new RelayBinding instance if successful, <c>null</c> otherwise.</returns>
		/// <param name="listener">Listener.</param>
		/// <param name="allowDuplicates">If <c>false</c>, checks whether persistent listener is already present.</param>
		public IRelayBinding BindListener(TDelegate listener, bool allowDuplicates=false){
			if (AddListener(listener, allowDuplicates)){
				return new RelayBinding<TDelegate>(this, listener, allowDuplicates, true);
			}
			return null;
		}
		/// <summary>
		/// Adds a one-time listener.
		/// These listeners are removed after one Dispatch.
		/// </summary>
		/// <returns><c>True</c> if successfully added listener, <c>false</c> otherwise</returns>
		/// <param name="listener">Listener.</param>
		/// <param name="allowDuplicates">If <c>false</c>, checks whether one-time listener is already present.</param>
		public bool AddOnce(TDelegate listener, bool allowDuplicates=false){
			if (!allowDuplicates && Contains(_listenersOnce, _onceCount, listener)) return false;
			if (_onceCount == _onceCap){
				if (_onceCap == 0){
					_onceCap = 1;
				} else {
					_onceCap *= 2;
				}
				_listenersOnce = Expand(_listenersOnce, _onceCap, _onceCount);
			}
			_listenersOnce[_onceCount] = listener;
			++_onceCount;
			#if SIGTRAP_RELAY_DBG
			_RelayDebugger.DebugAddListener(this, listener);
			#endif
			return true;
		}
		/// <summary>
		/// Removes a persistent listener, if present.
		/// </summary>
		/// <returns><c>true</c>, if listener was removed, <c>false</c> otherwise.</returns>
		/// <param name="listener">Listener.</param>
		public bool RemoveListener(TDelegate listener){
			bool result = false;
			for (uint i=0; i<_count; ++i){
				if (_listeners[i].Equals(listener)) {
					RemoveAt(i);
					result = true;
					break;
				}
			}
			#if SIGTRAP_RELAY_DBG
			if (result) _RelayDebugger.DebugRemListener(this, listener);
			#endif
			return result;
		}
		/// <summary>
		/// Removes a one-time listener, if present.
		/// </summary>
		/// <returns><c>true</c>, if listener was removed, <c>false</c> otherwise.</returns>
		/// <param name="listener">Listener.</param>
		public bool RemoveOnce(TDelegate listener){
			bool result = false;
			for (uint i=0; i<_onceCount; ++i){
				if (_listenersOnce[i].Equals(listener)) {
					RemoveOnceAt(i);
					result = true;
					break;
				}
			}
			#if SIGTRAP_RELAY_DBG
			if (result) _RelayDebugger.DebugRemListener(this, listener);
			#endif
			return result;
		}
		#endregion
		/// <summary>
		/// Removes all listeners.
		/// </summary>
		/// <param name="removePersistentListeners">If set to <c>true</c> remove persistent listeners.</param>
		/// <param name="removeOneTimeListeners">If set to <c>true</c>, remove one-time listeners.</param>
		public void RemoveAll(bool removePersistentListeners=true, bool removeOneTimeListeners=true){
			if (removePersistentListeners) {
				// No count check since array always present and RemoveAll
				// expected to be used when user knows there are listeners
				#if SIGTRAP_RELAY_DBG
				for (int i=0; i<_listeners.Length; ++i){
					_RelayDebugger.DebugRemListener(this, _listeners[i]);
				}
				#endif
				Array.Clear(_listeners, 0, (int)_cap);
				_count = 0;
			}
			if (removeOneTimeListeners && _onceCount > 0){
				// Count check because array lazily instantiated
				Array.Clear(_listenersOnce, 0, (int)_onceCap);
				_onceCount = 0;
			}
		}

		#region Internal
		protected void RemoveAt(uint i){
			_count = RemoveAt(_listeners, _count, i);
		}
		protected void RemoveOnceAt(uint i){
			_onceCount = RemoveAt(_listenersOnce, _onceCount, i);
		}
		protected uint RemoveAt(TDelegate[] arr, uint count, uint i){
			--count;
			for (uint j=i; j<count; ++j){
				arr[j] = arr[j+1];
			}
			arr[count] = null;
			return count;
		}

		bool Contains(TDelegate[] arr, uint c, TDelegate d){
			for (uint i=0; i<c; ++i){
				if (arr[i].Equals(d)) {
					return true;
				}
			}
			return false;
		}
		TDelegate[] Expand(TDelegate[] arr, uint cap, uint count){
			TDelegate[] newArr = new TDelegate[cap];
			for (int i=0; i<count; ++i){
				newArr[i] = arr[i];
			}
			return newArr;
		}
		#endregion
	}

	#region Implementations
	public class Relay : RelayBase<Action>, IRelayLink {
		private IRelayLink _link = null;
		/// <summary>
		/// Get an IRelayLink object that wraps this Relay without allowing Dispatch.
		/// Provides a safe interface for classes outside the Relay's "owner".
		/// </summary>
		public IRelayLink link {
			get {
				if (!_hasLink){
					_link = new RelayLink(this);
					_hasLink = true;
				}
				return _link;
			}
		}

		public void Dispatch(){
			// Persistent listeners
			// Reversal allows self-removal during dispatch (doesn't skip next listener)
			// Reversal allows safe addition during dispatch (doesn't fire immediately)
			for (uint i=_count; i>0; --i){		
				if (i>_count) throw _eIOOR;		
				if (_listeners[i-1] != null){	
					_listeners[i-1]();
				} else {
					RemoveAt(i-1);
				}
			}
			// One-time listeners - reversed for safe addition and auto-removal
			for (uint i=_onceCount; i>0; --i){
				var l = _listenersOnce[i-1];
				// Remove first to prevent potential infinite loops
			#if SIGTRAP_RELAY_DBG
				_RelayDebugger.DebugRemListener(this, _listenersOnce[i-1]);
			#endif
				_onceCount = RemoveAt(_listenersOnce, _onceCount, i-1);
				if (l != null){
					l();
				}
			}
		}
	}
	public class Relay<T> : RelayBase<Action<T>>, IRelayLink<T> {
		private IRelayLink<T> _link = null;
		/// <summary>
		/// Get an IRelayLink object that wraps this Relay without allowing Dispatch.
		/// Provides a safe interface for classes outside the Relay's "owner".
		/// </summary>
		public IRelayLink<T> link {
			get {
				if (!_hasLink){
					_link = new RelayLink<T>(this);
					_hasLink = true;
				}
				return _link;
			}
		}

		public void Dispatch(T t){
			for (uint i=_count; i>0; --i){
				if (i>_count) throw _eIOOR;
				if (_listeners[i-1] != null){
					_listeners[i-1](t);
				} else {
					RemoveAt(i-1);
				}
			}
			for (uint i=_onceCount; i>0; --i){
				var l = _listenersOnce[i-1];
				// Remove first to prevent potential infinite loops
			#if SIGTRAP_RELAY_DBG
				_RelayDebugger.DebugRemListener(this, _listenersOnce[i-1]);
			#endif
				_onceCount = RemoveAt(_listenersOnce, _onceCount, i-1);
				if (l != null){
					l(t);
				}
			}
		}
	}
	public class Relay<T,U> : RelayBase<Action<T,U>>, IRelayLink<T, U> {
		private IRelayLink<T, U> _link = null;
		/// <summary>
		/// Get an IRelayLink object that wraps this Relay without allowing Dispatch.
		/// Provides a safe interface for classes outside the Relay's "owner".
		/// </summary>
		public IRelayLink<T, U> link {
			get {
				if (!_hasLink){
					_link = new RelayLink<T, U>(this);
					_hasLink = true;
				}
				return _link;
			}
		}
		
		public void Dispatch(T t, U u){
			for (uint i=_count; i>0; --i){
				if (i>_count) throw _eIOOR;
				if (_listeners[i-1] != null){
					_listeners[i-1](t, u);
				} else {
					RemoveAt(i-1);
				}
			}
			for (uint i=_onceCount; i>0; --i){
				var l = _listenersOnce[i-1];
				// Remove first to prevent potential infinite loops
			#if SIGTRAP_RELAY_DBG
				_RelayDebugger.DebugRemListener(this, _listenersOnce[i-1]);
			#endif
				_onceCount = RemoveAt(_listenersOnce, _onceCount, i-1);
				if (l != null){
					l(t, u);
				}
			}
		}
	}
	public class Relay<T,U,V> : RelayBase<Action<T,U,V>>, IRelayLink<T, U, V> {
		private IRelayLink<T, U, V> _link = null;
		/// <summary>
		/// Get an IRelayLink object that wraps this Relay without allowing Dispatch.
		/// Provides a safe interface for classes outside the Relay's "owner".
		/// </summary>
		public IRelayLink<T, U, V> link {
			get {
				if (!_hasLink){
					_link = new RelayLink<T, U, V>(this);
					_hasLink = true;
				}
				return _link;
			}
		}

		public void Dispatch(T t, U u, V v){
			for (uint i=_count; i>0; --i){
				if (i>_count) throw _eIOOR;
				if (_listeners[i-1] != null){
					_listeners[i-1](t, u, v);
				} else {
					RemoveAt(i-1);
				}
			}
			for (uint i=_onceCount; i>0; --i){
				var l = _listenersOnce[i-1];
				// Remove first to prevent potential infinite loops
			#if SIGTRAP_RELAY_DBG
				_RelayDebugger.DebugRemListener(this, _listenersOnce[i-1]);
			#endif
				_onceCount = RemoveAt(_listenersOnce, _onceCount, i-1);
				if (l != null){
					l(t, u, v);
				}
			}
		}
	}
	public class Relay<T,U,V,W> : RelayBase<Action<T,U,V,W>>, IRelayLink<T, U, V, W> {
		private IRelayLink<T, U, V, W> _link = null;
		/// <summary>
		/// Get an IRelayLink object that wraps this Relay without allowing Dispatch.
		/// Provides a safe interface for classes outside the Relay's "owner".
		/// </summary>
		public IRelayLink<T, U, V, W> link {
			get {
				if (!_hasLink){
					_link = new RelayLink<T, U, V, W>(this);
					_hasLink = true;
				}
				return _link;
			}
		}

		public void Dispatch(T t, U u, V v, W w){
			for (uint i=_count; i>0; --i){
				if (i>_count) throw _eIOOR;
				if (_listeners[i-1] != null){
					_listeners[i-1](t, u, v, w);
				} else {
					RemoveAt(i-1);
				}
			}
			for (uint i=_onceCount; i>0; --i){
				var l = _listenersOnce[i-1];
				// Remove first to prevent potential infinite loops
			#if SIGTRAP_RELAY_DBG
				_RelayDebugger.DebugRemListener(this, _listenersOnce[i-1]);
			#endif
				_onceCount = RemoveAt(_listenersOnce, _onceCount, i-1);
				if (l != null){
					l(t, u, v, w);
				}
			}
		}
	}
	#endregion
}

#region Interface
namespace cfEngine.Util {
	public interface IRelayBinding {
		/// <summary>
		/// Is the listener currently subscribed to the Relay?
		/// </summary>
		bool enabled {get;}
		/// <summary>
		/// Should enabling the binding add the listener to the Relay if already added elsewhere?
		/// </summary>
		bool allowDuplicates {get; set;}
		/// <summary>
		/// How many persistent listeners does the bound Relay currently have?
		/// </summary>
		uint listenerCount {get;}

		/// <summary>
		/// Enable or disable the listener on the bound Relay.
		/// </summary>
		/// <returns><c>True</c> if listener was enabled/disabled successfully, <c>false</c> otherwise.true</returns>
		bool Enable(bool enable);
	}
}
#endregion

#region Implementation
namespace cfEngine.Util {
	public class RelayBinding<TDelegate> : IRelayBinding where TDelegate:class {
		protected IRelayLinkBase<TDelegate> _relay {get; private set;}
		protected TDelegate _listener {get;	private set;}

		#region Constructors
		private RelayBinding(){}	// Private empty constructor to force use of params
		public RelayBinding(IRelayLinkBase<TDelegate> relay, TDelegate listener, bool allowDuplicates, bool isListening) : this(){
			_relay = relay;
			_listener = listener;
			this.allowDuplicates = allowDuplicates;
			enabled = isListening;
		}
		#endregion

		#region IRelayBinding implementation
		/// <summary>
		/// Is the listener currently subscribed to the Relay?
		/// </summary>
		public bool enabled {get; private set;}
		/// <summary>
		/// Should enabling the binding add the listener to the Relay if already added elsewhere?
		/// </summary>
		public bool allowDuplicates {get; set;}
		/// <summary>
		/// How many persistent listeners does the bound Relay currently have?
		/// </summary>
		public uint listenerCount {get {return _relay.listenerCount;}}		

		/// <summary>
		/// Enable or disable the listener on the bound Relay.
		/// </summary>
		/// <returns><c>True</c> if listener was enabled/disabled successfully, <c>false</c> otherwise.true</returns>
		public bool Enable(bool enable){
			if (enable){
				if (!enabled){
					if (_relay.AddListener(_listener, allowDuplicates)){
						enabled = true;
						return true;
					}					
				}
			} else {
				if (enabled){
					if (_relay.RemoveListener(_listener)){
						enabled = false;
						return true;
					}
				}
			}
			return false;
		}
		#endregion
	}
}
#endregion

#region Interfaces
namespace cfEngine.Util {
	public interface IRelayLinkBase<TDelegate> where TDelegate:class {
		/// <summary>
		/// How many persistent listeners does this intance currently have?
		/// </summary>
		uint listenerCount {get;}
		/// <summary>
		/// How many one-time listeners does this intance currently have?
		/// After dispatch, all current one-time listeners are automatically removed.
		/// </summary>
		uint oneTimeListenersCount {get;}

		/// <summary>
		/// Is this delegate already a persistent listener?
		/// Does NOT query one-time listeners.
		/// </summary>
		/// <param name="listener">Listener.</param>
		bool Contains(TDelegate listener);

		/// <summary>
		/// Adds a persistent listener.
		/// </summary>
		/// <returns><c>True</c> if successfully added listener, <c>false</c> otherwise</returns>
		/// <param name="listener">Listener.</param>
		/// <param name="allowDuplicates">If <c>false</c>, checks whether persistent listener is already present.</param>
		bool AddListener(TDelegate listener, bool allowDuplicates=false);
		/// <summary>
		/// Adds listener and creates a RelayBinding between the listener and the Relay.
		/// The RelayBinding can be used to enable/disable the listener.
		/// </summary>
		/// <returns>A new RelayBinding instance if successful, <c>null</c> otherwise.</returns>
		/// <param name="listener">Listener.</param>
		/// <param name="allowDuplicates">If <c>false</c>, checks whether persistent listener is already present.</param>
		IRelayBinding BindListener(TDelegate listener, bool allowDuplicates=false);
		/// <summary>
		/// Adds a one-time listener.
		/// These listeners are removed after one Dispatch.
		/// </summary>
		/// <returns><c>True</c> if successfully added listener, <c>false</c> otherwise</returns>
		/// <param name="listener">Listener.</param>
		/// /// <param name="allowDuplicates">If <c>false</c>, checks whether one-time listener is already present.</param>
		bool AddOnce(TDelegate listener, bool allowDuplicates=false);

		/// <summary>
		/// Removes a persistent listener, if present.
		/// </summary>
		/// <returns><c>true</c>, if listener was removed, <c>false</c> otherwise.</returns>
		/// <param name="listener">Listener.</param>
		bool RemoveListener(TDelegate listener);
		/// <summary>
		/// Removes a listener added with AddOnce, if present.
		/// </summary>
		/// <returns><c>true</c>, if listener was removed, <c>false</c> otherwise.</returns>
		/// <param name="listener">Listener.</param>
		bool RemoveOnce(TDelegate listener);
		/// <summary>
		/// Removes all listeners.
		/// </summary>
		/// <param name="removePersistentListeners">If set to <c>true</c> remove persistent listeners.</param>
		/// <param name="removeOneTimeListeners">If set to <c>true</c>, also remove one-time listeners.</param>
		void RemoveAll(bool removePersistentListeners=true, bool removeOneTimeListeners=true);
	}
	public interface IRelayLink : IRelayLinkBase<Action> {}
	public interface IRelayLink<T> : IRelayLinkBase<Action<T>> {}
	public interface IRelayLink<T, U> : IRelayLinkBase<Action<T, U>> {}
	public interface IRelayLink<T, U, V> : IRelayLinkBase<Action<T, U, V>> {}
	public interface IRelayLink<T, U, V, W> : IRelayLinkBase<Action<T, U, V, W>> {}	
}
#endregion

#region Implementation
namespace cfEngine.Util {
	public abstract class RelayLinkBase<TDelegate> : IRelayLinkBase<TDelegate> where TDelegate:class {
		protected RelayBase<TDelegate> _relay;

		#region Constructors
		private RelayLinkBase(){}	// Private empty constructor to force use of params
		public RelayLinkBase(RelayBase<TDelegate> relay){
			_relay = relay;
		}
		#endregion

		#region IRelayLinkBase implementation
		public uint listenerCount {get {return _relay.listenerCount;}}
		public uint oneTimeListenersCount {get {return _relay.oneTimeListenersCount;}}

		/// <summary>
		/// Is this delegate already a persistent listener?
		/// Does NOT query one-time listeners.
		/// </summary>
		/// <param name="listener">Listener.</param>
		public bool Contains(TDelegate listener){
			return _relay.Contains(listener);
		}
		/// <summary>Adds a persistent listener.</summary>
		/// <param name="listener">Listener.</param>
		/// <param name="allowDuplicates">If <c>false</c>, checks whether persistent listener is already present.</param>
		public bool AddListener(TDelegate listener, bool allowDuplicates=false){
			return _relay.AddListener(listener, allowDuplicates);
		}
		/// <summary>
		/// Adds listener and creates a RelayBinding between the listener and the Relay.
		/// The RelayBinding can be used to enable/disable the listener.
		/// </summary>
		/// <returns>A new RelayBinding instance if successful, <c>null</c> otherwise.</returns>
		/// <param name="listener">Listener.</param>
		/// <param name="allowDuplicates">If <c>false</c>, checks whether persistent listener is already present.</param>
		public IRelayBinding BindListener(TDelegate listener, bool allowDuplicates=false){
			return _relay.BindListener(listener, allowDuplicates);
		}
		/// <summary>
		/// Adds a one-time listener.
		/// These listeners are removed after one Dispatch.
		/// </summary>
		/// <param name="listener">Listener.</param>
		/// /// <param name="allowDuplicates">If <c>false</c>, checks whether one-time listener is already present.</param>
		public bool AddOnce(TDelegate listener, bool allowDuplicates=false){
			return _relay.AddOnce(listener, allowDuplicates);
		}
		/// <summary>Removes a persistent listener, if present.</summary>
		/// <returns><c>true</c>, if listener was removed, <c>false</c> otherwise.</returns>
		/// <param name="listener">Listener.</param>
		public bool RemoveListener(TDelegate listener){
			return _relay.RemoveListener(listener);
		}
		/// <summary>Removes a listener added with AddOnce, if present.</summary>
		/// <returns><c>true</c>, if listener was removed, <c>false</c> otherwise.</returns>
		/// <param name="listener">Listener.</param>
		public bool RemoveOnce(TDelegate listener){
			return _relay.RemoveOnce(listener);
		}
		/// <summary>Removes all listeners.</summary>
		/// <param name="removePersistentListeners">If set to <c>true</c> remove persistent listeners.</param>
		/// <param name="removeOneTimeListeners">If set to <c>true</c>, also remove one-time listeners.</param>
		public void RemoveAll(bool removePersistentListeners=true, bool removeOneTimeListeners=true){
			_relay.RemoveAll(removePersistentListeners, removeOneTimeListeners);
		}
		#endregion
	}

	public class RelayLink : RelayLinkBase<Action>, IRelayLink {
		public RelayLink(RelayBase<Action> relay) : base(relay){}
	}
	public class RelayLink<T> : RelayLinkBase<Action<T>>, IRelayLink<T> {
		public RelayLink(RelayBase<Action<T>> relay) : base(relay){}
	}
	public class RelayLink<T, U> : RelayLinkBase<Action<T, U>>, IRelayLink<T, U> {
		public RelayLink(RelayBase<Action<T,U>> relay) : base(relay){}
	}
	public class RelayLink<T, U, V> : RelayLinkBase<Action<T, U, V>>, IRelayLink<T, U, V> {
		public RelayLink(RelayBase<Action<T,U,V>> relay) : base(relay){}
	}
	public class RelayLink<T, U, V, W> : RelayLinkBase<Action<T, U, V, W>>, IRelayLink<T, U, V, W> {
		public RelayLink(RelayBase<Action<T,U,V,W>> relay) : base(relay){}
	}
}
#endregion