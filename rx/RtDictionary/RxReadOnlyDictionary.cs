using System;
using System.Collections;
using System.Collections.Generic;
using cfEngine;

namespace cfEngine.Rx
{
    public abstract class RxReadOnlyDictionary<TKey, TValue> : RxCollection<KeyValuePair<TKey, TValue>>, IReadOnlyDictionary<TKey, TValue>
    {
        public abstract int Count { get; }

        public abstract bool ContainsKey(TKey key);

        public abstract bool TryGetValue(TKey key, out TValue value);

        public abstract TValue this[TKey key] { get; }

        public abstract IEnumerable<TKey> Keys { get; }

        public abstract IEnumerable<TValue> Values { get; }

        private RxReadOnlyList<KeyValuePair<TKey, TValue>> _rxPairs;
        
        public RxReadOnlyList<KeyValuePair<TKey, TValue>> rxPairs
        {
            get
            {
                if (_rxPairs == null)
                {
                    _rxPairs = new RxObserverList<KeyValuePair<TKey, TValue>>(this, CollectionEvents);
#if CF_REACTIVE_DEBUG
                    _rtPairs.__SetDebugName(nameof(RtPairs));
#endif
                }

                return _rxPairs;
            }
        }

        private RxReadOnlyList<TKey> _rxKeys;
        public RxReadOnlyList<TKey> rxKeys
        {
            get
            {
                if (_rxKeys == null)
                {
                    _rxKeys = rxPairs.select(kvp => kvp.Key);
#if CF_REACTIVE_DEBUG
                    _rtKeys.__SetDebugName(nameof(RtKeys));
#endif
                }

                return _rxKeys;
            }
        }

        private RxReadOnlyList<TValue> _rxValues;
        public RxReadOnlyList<TValue> rxValues
        {
            get
            {
                if (_rxValues == null)
                {
                    _rxValues = rxPairs.select(kvp => kvp.Value);
#if CF_REACTIVE_DEBUG
                    _rtValues.__SetDebugName(nameof(RtValues));
#endif
                }

                return _rxValues;
            }
        }

        public abstract IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
