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
    public abstract class RtReadOnlyDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>, IDisposable
    {
        private CollectionEvents<KeyValuePair<TKey, TValue>> _collectionEvents;
        protected CollectionEvents<KeyValuePair<TKey, TValue>> CollectionEvents => _collectionEvents ??= new CollectionEvents<KeyValuePair<TKey, TValue>>();

        public ICollectionEvents<KeyValuePair<TKey, TValue>> Events => CollectionEvents;

        public abstract int Count { get; }

        public abstract bool ContainsKey(TKey key);

        public abstract bool TryGetValue(TKey key, out TValue value);

        public abstract TValue this[TKey key] { get; }

        public abstract IEnumerable<TKey> Keys { get; }

        public abstract IEnumerable<TValue> Values { get; }

        private RtReadOnlyList<KeyValuePair<TKey, TValue>> _rtPairs;
        
        public RtReadOnlyList<KeyValuePair<TKey, TValue>> RtPairs => _rtPairs ??= new RtObserverList<KeyValuePair<TKey, TValue>>(this, CollectionEvents);

        private RtReadOnlyList<TKey> _rtKeys;
        public RtReadOnlyList<TKey> RtKeys => _rtKeys ??= RtPairs.Select(kvp => kvp.Key);

        private RtReadOnlyList<TValue> _rtValues;
        public RtReadOnlyList<TValue> RtValues => _rtValues ??= RtPairs.Select(kvp => kvp.Value);

        public abstract IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public virtual void Dispose()
        {
            CollectionEvents.OnDisposeRelay.Dispatch();
        }
        
        ~RtReadOnlyDictionary()
        {
            if (_collectionEvents != null && (_collectionEvents.OnAddRelay.listenerCount > 0 ||
                                              _collectionEvents.OnRemoveRelay.listenerCount > 0 ||
                                              _collectionEvents.OnUpdateRelay.listenerCount > 0))
            {
                Log.LogError($"{this}.Finalizer, it was not disposed properly!");
                Dispose();
            }
        }
    }
}
