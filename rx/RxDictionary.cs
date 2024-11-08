using System;
using System.Collections;
using System.Collections.Generic;
using cfEngine.Logging;
using cfEngine.Util;

namespace cfEngine.Rx
{
    public abstract class RxReadOnlyDictionary<TKey, TValue>: IDictionary<TKey, TValue>, IDisposable
    {
        #region Rx Events
        protected Relay<(TKey key, TValue value)> OnAddRelay;
        protected Relay<(TKey key, TValue value)> OnRemoveRelay;
        protected Relay<(TKey key, TValue oldValue, TValue newValue)> OnUpdateRelay;

        public event Action<(TKey key, TValue value)> OnAdd
        {
            add => OnAddRelay.AddListener(value);
            remove => OnAddRelay.RemoveListener(value);
        }
        
        public event Action<(TKey key, TValue value)> OnRemove 
        {
            add => OnRemoveRelay.AddListener(value);
            remove => OnRemoveRelay.RemoveListener(value);
        }
        
        public event Action<(TKey key, TValue oldValue, TValue newValue)> OnUpdate
        {
            add => OnUpdateRelay.AddListener(value);
            remove => OnUpdateRelay.AddListener(value);
        } 
        #endregion

        #region abstract IDictionary Implementation 

        public abstract IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract void Add(KeyValuePair<TKey, TValue> item);
        public abstract void Clear();
        public abstract bool Contains(KeyValuePair<TKey, TValue> item);
        public abstract void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex);
        public abstract bool Remove(KeyValuePair<TKey, TValue> item);
        public abstract int Count { get; }
        public abstract bool IsReadOnly { get; }
        public abstract void Add(TKey key, TValue value);
        public abstract bool ContainsKey(TKey key);
        public abstract bool Remove(TKey key);
        public abstract bool TryGetValue(TKey key, out TValue value);
        public abstract TValue this[TKey key] { get; set; }
        public abstract ICollection<TKey> Keys { get; }
        public abstract ICollection<TValue> Values { get; }

        #endregion

        private readonly RxReadOnlyDictionary<TKey, TValue> _source;

        protected RxReadOnlyDictionary()
        {
        }
        
        public RxReadOnlyDictionary(RxReadOnlyDictionary<TKey, TValue> source)
        {
            _source = source;
        }

        public void Dispose()
        {
            if (_source != null)
            {
                
            }
        }
    }
    
    public class RxDictionary<TKey, TValue>: RxReadOnlyDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    {
        private Dictionary<TKey, TValue> _dictionary = new();

        #region Dictionary Implementation 

        public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        public override void Add(KeyValuePair<TKey, TValue> kvp)
        {
            var (key, value) = kvp;
            if (_dictionary.TryAdd(key, value))
            {
                OnAddRelay.Dispatch((key, value));
            }
            else
            {
                Log.LogException(new ArgumentException($"{key} already exist, cannot add"));
            }
        }

        public override void Clear()
        {
            foreach (var (key, value) in _dictionary)
            {
                _dictionary.Remove(key);
                OnRemoveRelay.Dispatch((key, value));
            }
        }

        public override bool Contains(KeyValuePair<TKey, TValue> kvp)
        {
            return _dictionary.TryGetValue(kvp.Key, out var value) && value.Equals(kvp.Value);
        }

        public override void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).CopyTo(array, arrayIndex);
        }

        public override bool Remove(KeyValuePair<TKey, TValue> kvp)
        {
            var (key, value) = kvp;
            if (_dictionary.TryGetValue(key, out var v) && v.Equals(value))
            {
                _dictionary.Remove(key);
                OnRemoveRelay.Dispatch((key, value));
                return true;
            }

            return false;
        }

        public override int Count => _dictionary.Count;
        public override bool IsReadOnly => false;
        
        public override void Add(TKey key, TValue value)
        {
            if (_dictionary.TryAdd(key, value))
            {
                OnAddRelay.Dispatch((key, value));
            }
            else
            {
                Log.LogException(new ArgumentException($"{key} already exist, cannot add"));
            }
        }

        public override bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public override bool Remove(TKey key)
        {
            if (!_dictionary.Remove(key, out var value)) return false;
            
            OnRemoveRelay.Dispatch((key, value));
            return true;

        }

        public override bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public override TValue this[TKey key]
        {
            get => _dictionary[key];
            set => _dictionary[key] = value;
        }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => _dictionary.Keys;
        public override ICollection<TValue> Values => _dictionary.Values;

        public override ICollection<TKey> Keys => _dictionary.Keys;
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => _dictionary.Values;

        #endregion

        
        public RxDictionary(RxReadOnlyDictionary<TKey, TValue> source): base(source) { }
    }
}