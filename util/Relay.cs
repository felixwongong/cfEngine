using System;
using System.Collections.Generic;
using System.Diagnostics;
using cfEngine;

namespace cfEngine.Rx
{
    public abstract partial class Subscription
    {
        public abstract void Unsubscribe();
    }

    public partial class SubscriptionGroup : Subscription
    {
        private List<Subscription> _subscriptions = new List<Subscription>();
        
        public void Add(Subscription subscription)
        {
            _subscriptions.Add(subscription);
        }
        
        public override void Unsubscribe()
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.UnsubscribeIfNotNull();
            }
            
            _subscriptions.Clear();
        }
    }
    
    public partial class SubscriptionBinding<TDelegate>: Subscription where TDelegate: class
    {
        public readonly TDelegate Listener;
        private readonly DelegateDispatcher<TDelegate> _relay;
        public SubscriptionBinding(TDelegate listener, DelegateDispatcher<TDelegate> relay)
        {
            Listener = listener;
            _relay = relay;
        }

        public bool IsListener(TDelegate d)
        {
            return Listener.Equals(d);
        }
        
        public override void Unsubscribe()
        {
            _relay?.RemoveListener(Listener);
        }
    }

    public static class SubscriptionHandleExtension
    {
        public static void UnsubscribeIfNotNull(this Subscription binding)
        {
            binding?.Unsubscribe();
        }
    }
    
    public interface IDelegateDispatcher<TDelegate> where TDelegate : class
    {
        Subscription AddListener(TDelegate listener);
        bool RemoveListener(TDelegate listener);
        void RemoveAll();
        bool Contains(TDelegate d);
    }
    
    public abstract class DelegateDispatcher<TDelegate>: IDelegateDispatcher<TDelegate> where TDelegate : class
    {
        protected WeakReference<SubscriptionBinding<TDelegate>>[] _subscriptionRefList;
        protected int _cap;
        protected int _count;

        public int listenerCount => _count;
        
#pragma warning disable 0414
        protected readonly object _o;
#pragma warning restore 0414

        public DelegateDispatcher(object owner, int defaultSize = 1)
        {
            _o = owner;
            _cap = defaultSize;
            _subscriptionRefList = new WeakReference<SubscriptionBinding<TDelegate>>[_cap];
        }

        public Subscription AddListener(TDelegate listener)
        {
            if (Contains(listener))
            {
                Log.LogError("DelegateDispatcher.AddListener: Listener already exists");
                return null;
            }

            if (_count == _cap)
            {
                _count = Expand(ref _subscriptionRefList);
            }

            var subscription = new SubscriptionBinding<TDelegate>(listener, this);
            var subscriptionRef = new WeakReference<SubscriptionBinding<TDelegate>>(subscription);
            _subscriptionRefList[_count++] = subscriptionRef;

#if CF_RX_INSTRUMENTED
            var weakSub = new WeakReference<Subscription>(subscription);
            RxInstrumentation.OnListenerSubscribed?.Invoke(_o, listener as Delegate, weakSub);
#endif

            return subscription;
        }

        public bool RemoveListener(TDelegate listener)
        {
            bool result = false;
            for (var i = 0; i < _cap; i++)
            {
                if (_subscriptionRefList[i] != null &&
                    _subscriptionRefList[i].TryGetTarget(out var subscription) &&
                    subscription.IsListener(listener))
                {
                    _subscriptionRefList[i] = null;
                    _count--;
                    result = true;

#if CF_RX_INSTRUMENTED
                    RxInstrumentation.OnListenerRemoved?.Invoke(_o, listener as Delegate);
#endif
                }
            }

            return result;
        }

        public void RemoveAll()
        {
            for (var i = 0; i < _subscriptionRefList.Length; i++)
            {
                _subscriptionRefList[i] = null;
            }

            _cap = _subscriptionRefList.Length;
        }

        public bool Contains(TDelegate d)
        {
            if (_cap > _subscriptionRefList.Length)
            {
                Log.LogException(new IndexOutOfRangeException("DelegateDispatcher.Contains: Bound exceeded the length of the subscription array"));
                return false;
            }

            return Contains(_subscriptionRefList.AsSpan(0, _cap), d);
        }

        private bool Contains(Span<WeakReference<SubscriptionBinding<TDelegate>>> bindings, TDelegate target)
        {
            foreach (var bindingRef in bindings)
            {
                if (bindingRef != null && bindingRef.TryGetTarget(out var binding) && binding.IsListener(target))
                {
                    return true;
                }
            }

            return false;
        }

        public int Expand(ref WeakReference<SubscriptionBinding<TDelegate>>[] bindings)
        {
            _cap *= 2;
            
            var newBindings = new WeakReference<SubscriptionBinding<TDelegate>>[_cap];
            var newCount = 0;
            for (var i = 0; i < bindings.Length; i++)
            {
                if (bindings[i] != null && bindings[i].TryGetTarget(out _))
                {
                   newBindings[newCount++] = bindings[i]; 
                }
            }
            
            bindings = newBindings;

            return newCount;
        }
    }

    public interface IRelay: IDelegateDispatcher<Action> { }
    public interface IRelay<T>: IDelegateDispatcher<Action<T>> { }
    public interface IRelay<T1, T2>: IDelegateDispatcher<Action<T1, T2>> { }
    
    public class Relay: DelegateDispatcher<Action>, IRelay
    {
        public Relay(object owner, int defaultSize = 1) : base(owner, defaultSize)
        {
        }
        public void Dispatch()
        {
            int newCount = 0;
            for (int i = 0; i < _cap;)
            {
                if (_subscriptionRefList[i] == null || !_subscriptionRefList[i].TryGetTarget(out var subscription))
                {
                    _subscriptionRefList[i] = null;
                    if (i + 1 < _cap)
                    {
                        _subscriptionRefList[i] = _subscriptionRefList[i + 1];
                        continue;
                    }
                }
                else
                {
#if CF_RX_INSTRUMENTED
                    var listenerDelegate = subscription.Listener as Delegate;
                    var sw = Stopwatch.StartNew();
                    RxInstrumentation.OnDispatching?.Invoke(_o, listenerDelegate, null);
                    Exception? ex = null;
                    try
                    {
                        subscription.Listener?.Invoke();
                    }
                    catch (Exception caught)
                    {
                        ex = caught;
                    }
                    sw.Stop();
                    long elapsedMicros = sw.ElapsedTicks * 1_000_000L / Stopwatch.Frequency;
                    RxInstrumentation.OnDispatched?.Invoke(_o, listenerDelegate, elapsedMicros, ex);
                    if (ex != null && RxInstrumentation.StrictMode)
                    {
                        throw ex;
                    }
#else
                    subscription.Listener?.Invoke();
#endif
                    newCount++;
                }

                i++;
            }

            _count = newCount;
        }
    }

    public class Relay<T>: DelegateDispatcher<Action<T>>, IRelay<T>
    {
        public Relay(object owner, int defaultSize = 1) : base(owner, defaultSize)
        {
        }
        public void Dispatch(T value1)
        {
            int newCount = 0;
            for (int i = 0; i < _cap;)
            {
                if (_subscriptionRefList[i] == null || !_subscriptionRefList[i].TryGetTarget(out var subscription))
                {
                    _subscriptionRefList[i] = null;
                    if (i + 1 < _cap)
                    {
                        _subscriptionRefList[i] = _subscriptionRefList[i + 1];
                        continue;
                    }
                }
                else
                {
#if CF_RX_INSTRUMENTED
                    var listenerDelegate = subscription.Listener as Delegate;
                    var sw = Stopwatch.StartNew();
                    RxInstrumentation.OnDispatching?.Invoke(_o, listenerDelegate, new object[] { value1 });
                    Exception? ex = null;
                    try
                    {
                        subscription.Listener?.Invoke(value1);
                    }
                    catch (Exception caught)
                    {
                        ex = caught;
                    }
                    sw.Stop();
                    long elapsedMicros = sw.ElapsedTicks * 1_000_000L / Stopwatch.Frequency;
                    RxInstrumentation.OnDispatched?.Invoke(_o, listenerDelegate, elapsedMicros, ex);
                    if (ex != null && RxInstrumentation.StrictMode)
                    {
                        throw ex;
                    }
#else
                    subscription.Listener?.Invoke(value1);
#endif
                    newCount++;
                }

                i++;
            }

            _count = newCount;
        }
    }

    public class Relay<T1, T2> : DelegateDispatcher<Action<T1, T2>>, IRelay<T1, T2>
    {
        public Relay(object owner, int defaultSize = 1) : base(owner, defaultSize)
        {
        }
        public void Dispatch(T1 value1, T2 value2)
        { 
            int newCount = 0;
            for (int i = 0; i < _cap;)
            {
                if (_subscriptionRefList[i] == null || !_subscriptionRefList[i].TryGetTarget(out var subscription))
                {
                    _subscriptionRefList[i] = null;
                    if (i + 1 < _cap)
                    {
                        _subscriptionRefList[i] = _subscriptionRefList[i + 1];
                        continue;
                    }
                }
                else
                {
#if CF_RX_INSTRUMENTED
                    var listenerDelegate = subscription.Listener as Delegate;
                    var sw = Stopwatch.StartNew();
                    RxInstrumentation.OnDispatching?.Invoke(_o, listenerDelegate, new object[] { value1, value2 });
                    Exception? ex = null;
                    try
                    {
                        subscription.Listener?.Invoke(value1, value2);
                    }
                    catch (Exception caught)
                    {
                        ex = caught;
                    }
                    sw.Stop();
                    long elapsedMicros = sw.ElapsedTicks * 1_000_000L / Stopwatch.Frequency;
                    RxInstrumentation.OnDispatched?.Invoke(_o, listenerDelegate, elapsedMicros, ex);
                    if (ex != null && RxInstrumentation.StrictMode)
                    {
                        throw ex;
                    }
#else
                    subscription.Listener?.Invoke(value1, value2);
#endif
                    newCount++;
                }

                i++;
            }

            _count = newCount;
        }
    }
}