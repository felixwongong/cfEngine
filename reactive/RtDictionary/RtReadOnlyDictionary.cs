using System;
using System.Collections;
using System.Collections.Generic;

namespace cfEngine.Rt
{
    /// <summary>
    /// Represents a read-only dictionary with event dispatching capabilities.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    public abstract class RtReadOnlyDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>, IDisposable
    {
        protected readonly CollectionEvents<KeyValuePair<TKey, TValue>> CollectionEvents = new();

        /// <summary>
        /// Gets the collection events.
        /// </summary>
        public ICollectionEvents<KeyValuePair<TKey, TValue>> Events => CollectionEvents;

        /// <summary>
        /// Gets the number of elements in the dictionary.
        /// </summary>
        public abstract int Count { get; }

        /// <summary>
        /// Determines whether the dictionary contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the dictionary.</param>
        /// <returns>true if the dictionary contains an element with the specified key; otherwise, false.</returns>
        public abstract bool ContainsKey(TKey key);

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter.</param>
        /// <returns>true if the dictionary contains an element with the specified key; otherwise, false.</returns>
        public abstract bool TryGetValue(TKey key, out TValue value);

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <returns>The value associated with the specified key.</returns>
        public abstract TValue this[TKey key] { get; }

        /// <summary>
        /// Gets an enumerable collection that contains the keys in the dictionary.
        /// </summary>
        public abstract IEnumerable<TKey> Keys { get; }

        /// <summary>
        /// Gets an enumerable collection that contains the values in the dictionary.
        /// </summary>
        public abstract IEnumerable<TValue> Values { get; }

        private RtReadOnlyList<KeyValuePair<TKey, TValue>> _rtPairs;
        /// <summary>
        /// Gets the read-only list of key-value pairs.
        /// </summary>
        public RtReadOnlyList<KeyValuePair<TKey, TValue>> RtPairs => _rtPairs ??= new RtObserverList<KeyValuePair<TKey, TValue>>(this, CollectionEvents);

        private RtReadOnlyList<TKey> _rtKeys;
        /// <summary>
        /// Gets the read-only list of keys.
        /// </summary>
        public RtReadOnlyList<TKey> RtKeys => _rtKeys ??= RtPairs.Select(kvp => kvp.Key);

        private RtReadOnlyList<TValue> _rtValues;
        /// <summary>
        /// Gets the read-only list of values.
        /// </summary>
        public RtReadOnlyList<TValue> RtValues => _rtValues ??= RtPairs.Select(kvp => kvp.Value);

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public abstract IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="RtReadOnlyDictionary{TKey, TValue}"/> class.
        /// </summary>
        public virtual void Dispose()
        {
            CollectionEvents.OnDisposeRelay.Dispatch();
            CollectionEvents.Dispose();
        }
    }
}
