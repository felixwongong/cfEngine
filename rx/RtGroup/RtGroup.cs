using System;
using System.Collections.Generic;
using cfEngine.Logging;

namespace cfEngine.Rt
{
    public class RtGroup<TKey, TValue>: RtReadOnlyDictionary<TKey, RtReadOnlyList<TValue>>
    {
        private readonly ICollectionEvents<(int index, TValue item)> _sourceEvent;
        private readonly Func<TValue, TKey> _keyFn;
        
        private readonly Dictionary<TKey, RtList<TValue>> _groups = new();

        public RtGroup(RtReadOnlyList<TValue> source, Func<TValue, TKey> keyFn)
        {
            _sourceEvent = source.Events;
            _keyFn = keyFn;

            foreach (var item in source)
            {
                var key = keyFn(item);
                if (!_groups.TryGetValue(key, out var group))
                {
                    group = new RtList<TValue>(1);
                    _groups.Add(key, group);
                }
                
                group.Add(item);
            }
            
            _sourceEvent.Subscribe(OnSourceAdd, OnSourceRemove, OnSourceUpdate, Dispose);
        }

        private void OnSourceUpdate((int index, TValue item) oldListItem, (int index, TValue item) newListItem)
        {
            OnSourceRemove(oldListItem);
            OnSourceAdd(newListItem);
        }

        private void OnSourceRemove((int index, TValue item) listItem)
        {
            var (index, item) = listItem;
            var key = _keyFn(item);

            if (!_groups.TryGetValue(key, out var group))
            {
                Log.LogException(new KeyNotFoundException($"Group not found on source removed, key: {key.ToString()}, item: {item.ToString()}"));
                return;
            }

            if (!group.Remove(item))
            {
                Log.LogException(new ArgumentException($"Group ({key}) cannot remove item {item.ToString()}"));
                return;
            }

            if (group.Count <= 0)
            {
                _groups.Remove(key);
                CollectionEvents.OnRemoveRelay.Dispatch(new KeyValuePair<TKey, RtReadOnlyList<TValue>>(key, group));
                group.Dispose();
            }
        }

        private void OnSourceAdd((int index, TValue item) listItem)
        {
            var (index, item) = listItem;
            
            var key = _keyFn(item);
            if (!_groups.TryGetValue(key, out var group))
            {
                group = new RtList<TValue>(1);
                _groups.Add(key, group);
            }
            
            group.Add(item);
            
            CollectionEvents.OnAddRelay.Dispatch(new KeyValuePair<TKey, RtReadOnlyList<TValue>>(key, group));
        }

        public override IEnumerator<KeyValuePair<TKey, RtReadOnlyList<TValue>>> GetEnumerator()
        {
            foreach (var (key, group) in _groups)
            {
                yield return new KeyValuePair<TKey, RtReadOnlyList<TValue>>(key, group);
            }
        }

        public override int Count => _groups.Count;
        public override bool ContainsKey(TKey key)
        {
            return _groups.ContainsKey(key);
        }

        public override bool TryGetValue(TKey key, out RtReadOnlyList<TValue> value)
        {
            value = null;
            if (!_groups.TryGetValue(key, out var group))
            {
                return false;
            }

            value = group;
            return true;
        }

        public override RtReadOnlyList<TValue> this[TKey key] => _groups[key];

        public override IEnumerable<TKey> Keys => _groups.Keys;
        public override IEnumerable<RtReadOnlyList<TValue>> Values => _groups.Values;

        public override void Dispose()
        {
            foreach (var group in _groups.Values)
            {
                group.Dispose();
            }
            
            CollectionEvents.OnDisposeRelay.Dispatch();
            
            _sourceEvent.Unsubscribe(OnSourceAdd, OnSourceRemove, OnSourceUpdate, Dispose);

            base.Dispose();
        }
    }
}