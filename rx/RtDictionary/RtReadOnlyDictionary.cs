using System;
using System.Collections;
using System.Collections.Generic;

namespace cfEngine.Rx
{
    public abstract class RtReadOnlyDictionary<TKey, TValue>: IReadOnlyDictionary<TKey, TValue>, IDisposable
    {
        protected readonly CollectionEvents<KeyValuePair<TKey, TValue>> CollectionEvents = new();
        public ICollectionEvents<KeyValuePair<TKey, TValue>> Events => CollectionEvents;

        public abstract IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator();
        public abstract int Count { get; }
        public abstract bool ContainsKey(TKey key);
        public abstract bool TryGetValue(TKey key, out TValue value);
        public abstract TValue this[TKey key] { get; }
        public abstract IEnumerable<TKey> Keys { get; }
        public abstract IEnumerable<TValue> Values { get; }

        private RtReadOnlyList<TKey> _rtKeys;
        public RtReadOnlyList<TKey> RtKeys => _rtKeys ??= new RtList<TKey>(Keys);
        private RtReadOnlyList<TValue> _rtValues;
        public RtReadOnlyList<TValue> RtValues => _rtValues ??= new RtList<TValue>(Values);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        
        public virtual void Dispose()
        {
            CollectionEvents.Dispose();
        }
    }
}