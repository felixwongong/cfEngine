using System;
using System.Collections;
using System.Collections.Generic;
using cfEngine.Logging;

namespace cfEngine.Rt
{
    public abstract class RtReadOnlyDictionary<TKey, TValue> : RtCollection<KeyValuePair<TKey, TValue>>, IReadOnlyDictionary<TKey, TValue>
    {
        public abstract int Count { get; }

        public abstract bool ContainsKey(TKey key);

        public abstract bool TryGetValue(TKey key, out TValue value);

        public abstract TValue this[TKey key] { get; }

        public abstract IEnumerable<TKey> Keys { get; }

        public abstract IEnumerable<TValue> Values { get; }

        private RtReadOnlyList<KeyValuePair<TKey, TValue>> _rtPairs;
        
        public RtReadOnlyList<KeyValuePair<TKey, TValue>> RtPairs
        {
            get
            {
                if (_rtPairs == null)
                {
                    _rtPairs = new RtObserverList<KeyValuePair<TKey, TValue>>(this, CollectionEvents);
#if CF_REACTIVE_DEBUG
                    _rtPairs.__SetDebugName(nameof(RtPairs));
#endif
                }

                return _rtPairs;
            }
        }

        private RtReadOnlyList<TKey> _rtKeys;
        public RtReadOnlyList<TKey> RtKeys
        {
            get
            {
                if (_rtKeys == null)
                {
                    _rtKeys = RtPairs.select(kvp => kvp.Key);
#if CF_REACTIVE_DEBUG
                    _rtKeys.__SetDebugName(nameof(RtKeys));
#endif
                }

                return _rtKeys;
            }
        }

        private RtReadOnlyList<TValue> _rtValues;
        public RtReadOnlyList<TValue> RtValues
        {
            get
            {
                if (_rtValues == null)
                {
                    _rtValues = RtPairs.select(kvp => kvp.Value);
#if CF_REACTIVE_DEBUG
                    _rtValues.__SetDebugName(nameof(RtValues));
#endif
                }

                return _rtValues;
            }
        }

        public abstract IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
