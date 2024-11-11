using System;
using System.Collections.Generic;

namespace cfEngine.Rt
{
    public class RtFilteredDictionary<TKey, TValue>: RtReadOnlyDictionary<TKey, TValue>
    {
        private readonly ICollectionEvents<KeyValuePair<TKey, TValue>> _sourceEvents;
        private readonly Func<KeyValuePair<TKey, TValue>, bool> _filterFn;

        private readonly Dictionary<TKey, TValue> _filtered = new();

        public RtFilteredDictionary(RtReadOnlyDictionary<TKey, TValue> source, Func<KeyValuePair<TKey, TValue>, bool> filterFn)
        {
            _filterFn = filterFn;
            _sourceEvents = source.Events;
            
            foreach (var kvp in source)
            {
                if(!filterFn(kvp)) continue;
                
                _filtered.Add(kvp.Key, kvp.Value);
            }
            
            source.Events.Subscribe(OnSourceAdd, OnSourceRemove, OnSourceUpdate, Dispose);
        }

        private void OnSourceUpdate(KeyValuePair<TKey, TValue> oldPair, KeyValuePair<TKey, TValue> newPair)
        {
        }

        private void OnSourceRemove(KeyValuePair<TKey, TValue> kvp)
        {
            
        }

        private void OnSourceAdd(KeyValuePair<TKey, TValue> kvp)
        {
        }

        public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        public override int Count { get; }
        public override bool ContainsKey(TKey key)
        {
            throw new System.NotImplementedException();
        }

        public override bool TryGetValue(TKey key, out TValue value)
        {
            throw new System.NotImplementedException();
        }

        public override TValue this[TKey key] => throw new System.NotImplementedException();

        public override IEnumerable<TKey> Keys { get; }
        public override IEnumerable<TValue> Values { get; }

        public override void Dispose()
        {
            base.Dispose();
            
            _sourceEvents.Unsubscribe(OnSourceAdd, OnSourceRemove, OnSourceUpdate, Dispose);
            _filtered.Clear();
        }
    }
}