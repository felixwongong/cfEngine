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
            bool canAdd, canRemove;

            canRemove = _filtered.TryGetValue(oldPair.Key, out var oldValue) && oldValue.Equals(oldPair.Value);
            canAdd = _filterFn(newPair);

            if (canRemove && canAdd && oldPair.Key.Equals(newPair.Key))
            {
                _filtered[oldPair.Key] = newPair.Value;
                CollectionEvents.OnUpdateRelay.Dispatch(oldPair, newPair);
            } else if (canAdd)
            {
                _filtered[oldPair.Key] = newPair.Value;
                CollectionEvents.OnAddRelay.Dispatch(newPair);
            } else if (canRemove)
            {
                _filtered.Remove(oldPair.Key);
                CollectionEvents.OnRemoveRelay.Dispatch(oldPair);
            }
        }

        private void OnSourceRemove(KeyValuePair<TKey, TValue> kvp)
        {
            if (_filtered.TryGetValue(kvp.Key, out var oldValue) && oldValue.Equals(kvp.Value))
            {
                _filtered.Remove(kvp.Key);
                CollectionEvents.OnRemoveRelay.Dispatch(kvp);
            }
        }

        private void OnSourceAdd(KeyValuePair<TKey, TValue> kvp)
        {
            if (_filterFn(kvp))
            {
                _filtered[kvp.Key] = kvp.Value;
                CollectionEvents.OnAddRelay.Dispatch(kvp);
            }
        }

        public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _filtered.GetEnumerator();
        }

        public override int Count => _filtered.Count;
        public override bool ContainsKey(TKey key)
        {
            return _filtered.ContainsKey(key);
        }

        public override bool TryGetValue(TKey key, out TValue value)
        {
            return _filtered.TryGetValue(key, out value);
        }

        public override TValue this[TKey key] => _filtered[key];

        public override IEnumerable<TKey> Keys => _filtered.Keys;
        public override IEnumerable<TValue> Values => _filtered.Values;

        public override void Dispose()
        {
            base.Dispose();
            
            _sourceEvents.Unsubscribe(OnSourceAdd, OnSourceRemove, OnSourceUpdate, Dispose);
            _filtered.Clear();
        }
    }
}