using System;
using System.Collections.Generic;
using cfEngine.Logging;

namespace cfEngine.Rx
{
    public class RtDictionary<TKey, TValue>: RtReadOnlyDictionary<TKey, TValue>
    {
        private Dictionary<TKey, TValue> _dictionary = new();

        #region Dictionary Implementation 
        
        public override TValue this[TKey key] => _dictionary[key];
        public override IEnumerable<TKey> Keys => _dictionary.Keys;
        public override IEnumerable<TValue> Values => _dictionary.Values;

        public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
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

        public override int Count => _dictionary.Count;
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

        public override bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            if (!_dictionary.Remove(key, out var value)) return false;
            
            OnRemoveRelay.Dispatch((key, value));
            return true;

        }

        public override bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public void Upsert(TKey key, TValue value)
        {
            if (_dictionary.TryGetValue(key, out var oldValue))
            {
                _dictionary[key] = value;
                OnUpdateRelay.Dispatch((key, oldValue, value));
            }
            else
            {
                _dictionary[key] = value;
                OnAddRelay.Dispatch((key, value));
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            _dictionary.Clear();
        }

        #endregion
    }
}