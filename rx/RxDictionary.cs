using System;
using System.Collections;
using System.Collections.Generic;
using cfEngine.Logging;
using cfEngine.Util;

namespace cfEngine.Rx
{
    public class RxDictionary<TKey, TValue>: IDictionary<TKey, TValue>
    {
        private Dictionary<TKey, TValue> _dictionary = new();

        #region Rx Events
        private Relay<(TKey key, TValue value)> OnAddRelay;
        private Relay<(TKey key, TValue value)> OnRemoveRelay;
        private Relay<(TKey key, TValue oldValue, TValue newValue)> OnUpdateRelay;

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

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> kvp)
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

        public void Clear()
        {
            foreach (var (key, value) in _dictionary)
            {
                _dictionary.Remove(key);
                OnRemoveRelay.Dispatch((key, value));
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> kvp)
        {
            return _dictionary.TryGetValue(kvp.Key, out var value) && value.Equals(kvp.Value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> kvp)
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

        public int Count => _dictionary.Count;
        public bool IsReadOnly => false;
        
        public void Add(TKey key, TValue value)
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

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            if (!_dictionary.Remove(key, out var value)) return false;
            
            OnRemoveRelay.Dispatch((key, value));
            return true;

        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get => _dictionary[key];
            set => _dictionary[key] = value;
        }

        public ICollection<TKey> Keys => _dictionary.Keys;
        public ICollection<TValue> Values => _dictionary.Values;
    }
}