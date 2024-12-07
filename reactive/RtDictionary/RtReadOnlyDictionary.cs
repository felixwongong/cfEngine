using System;
using System.Collections;
using System.Collections.Generic;
using cfEngine.Logging;

namespace cfEngine.Rt
{
    /// <summary>
    /// Represents a read-only dictionary with event dispatching capabilities.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    public abstract partial class RtReadOnlyDictionary<TKey, TValue> : RtCollection<KeyValuePair<TKey, TValue>>, IReadOnlyDictionary<TKey, TValue>, IDisposable
    {
        public abstract int Count { get; }

        public abstract bool ContainsKey(TKey key);

        public abstract bool TryGetValue(TKey key, out TValue value);

        public abstract TValue this[TKey key] { get; }

        public abstract IEnumerable<TKey> Keys { get; }

        public abstract IEnumerable<TValue> Values { get; }

        private RtReadOnlyList<KeyValuePair<TKey, TValue>> _rtPairs;
        
        public RtReadOnlyList<KeyValuePair<TKey, TValue>> RtPairs => _rtPairs ??= new RtObserverList<KeyValuePair<TKey, TValue>>(this, CollectionEvents);

        private RtReadOnlyList<TKey> _rtKeys;
        public RtReadOnlyList<TKey> RtKeys => _rtKeys ??= RtPairs.select(kvp => kvp.Key);

        private RtReadOnlyList<TValue> _rtValues;
        public RtReadOnlyList<TValue> RtValues => _rtValues ??= RtPairs.select(kvp => kvp.Value);

        public abstract IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
