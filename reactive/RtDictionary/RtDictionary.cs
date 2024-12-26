using System;
using System.Collections.Generic;
using cfEngine.Logging;

namespace cfEngine.Rt
{
    /// <summary>
    /// Represents a dictionary that supports mutation and event dispatching.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    public class RtDictionary<TKey, TValue> : RtReadOnlyDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _dictionary = new();
        
        public override void Dispose()
        {
            base.Dispose();
            _dictionary.Clear();
        }
        
        #region Dictionary Implementation

        public override TValue this[TKey key] => _dictionary[key];
        public override IEnumerable<TKey> Keys => _dictionary.Keys;
        public override IEnumerable<TValue> Values => _dictionary.Values;

        public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        /// <summary>
        /// Adds a key-value pair to the dictionary.
        /// </summary>
        /// <param name="kvp">The key-value pair to add.</param>
        /// <exception cref="ArgumentException">Thrown when the key already exists.</exception>
        public void Add(KeyValuePair<TKey, TValue> kvp)
        {
            var (key, value) = kvp;
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (!_dictionary.TryAdd(key, value))
            {
                Log.LogException(new ArgumentException($"{key} already exists, cannot add"));
                return;
            }

            CollectionEvents.OnAddRelay.Dispatch(kvp);
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
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (_dictionary.TryGetValue(kvp.Key, out var v) && v.Equals(kvp.Value))
            {
                _dictionary.Remove(key);
                CollectionEvents.OnRemoveRelay.Dispatch(kvp);
                return true;
            }

            return false;
        }

        public override int Count => _dictionary.Count;
        public bool IsReadOnly => false;

        public override bool ContainsKey(TKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            return _dictionary.ContainsKey(key);
        }

        public override bool TryGetValue(TKey key, out TValue value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            return _dictionary.TryGetValue(key, out value);
        }

        public void Add(TKey key, TValue value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (!_dictionary.TryAdd(key, value))
            {
                Log.LogException(new ArgumentException($"{key} already exists, cannot add"));
                return;
            }

            CollectionEvents.OnAddRelay.Dispatch(new KeyValuePair<TKey, TValue>(key, value));
        }

        public void Upsert(TKey key, TValue value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (_dictionary.TryGetValue(key, out var oldValue))
            {
                _dictionary[key] = value;
                CollectionEvents.OnUpdateRelay.Dispatch(
                    new KeyValuePair<TKey, TValue>(key, oldValue),
                    new KeyValuePair<TKey, TValue>(key, value)
                );
            }
            else
            {
                Add(new KeyValuePair<TKey, TValue>(key, value));
            }
        }

        public bool Remove(TKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (!_dictionary.Remove(key, out var value)) return false;

            CollectionEvents.OnRemoveRelay.Dispatch(new KeyValuePair<TKey, TValue>(key, value));
            return true;
        }

        public void Clear()
        {
            var items = new List<KeyValuePair<TKey, TValue>>(_dictionary);
            _dictionary.Clear();
            foreach (var kvp in items)
            {
                CollectionEvents.OnRemoveRelay.Dispatch(kvp);
            }
        }

        #endregion
    }
}
