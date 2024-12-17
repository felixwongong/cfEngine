using System;
using System.Collections.Generic;
using cfEngine.Logging;
using cfEngine.Util;

namespace cfEngine.Rt
{
    public class RtGroup<TKey, TValue> : RtReadOnlyDictionary<TKey, RtReadOnlyList<TValue>>
    {
        private readonly Func<TValue, TKey> _keyFn;
        private readonly Dictionary<TKey, RtList<TValue>> _groups = new();
        
        private Subscription _sourceChangeSubscription;

        public RtGroup(RtReadOnlyList<TValue> source, Func<TValue, TKey> keyFn)
        {
            var sourceEvent = source.Events;
            _keyFn = keyFn ?? throw new ArgumentNullException(nameof(keyFn));

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

            _sourceChangeSubscription = sourceEvent.Subscribe(OnSourceAdd, OnSourceRemove, OnSourceUpdate, Dispose);

#if CF_REACTIVE_DEBUG
            __SetSourceCollectionId(source);
#endif
        }

        public override void Dispose()
        {
            base.Dispose();
            _sourceChangeSubscription.UnsubscribeIfNotNull();

            foreach (var group in _groups.Values)
            {
                group.Dispose();
            }

            _groups.Clear();
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
                Log.LogException(new KeyNotFoundException($"Group not found on source removed, key: {key}, item: {item}"));
                return;
            }

            if (!group.Remove(item))
            {
                Log.LogException(new ArgumentException($"Group ({key}) cannot remove item {item}"));
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

        public override bool ContainsKey(TKey key) => _groups.ContainsKey(key);

        public override bool TryGetValue(TKey key, out RtReadOnlyList<TValue> value)
        {
            if (_groups.TryGetValue(key, out var group))
            {
                value = group;
                return true;
            }

            value = null;
            return false;
        }

        public override RtReadOnlyList<TValue> this[TKey key] => _groups[key];

        public override IEnumerable<TKey> Keys => _groups.Keys;

        public override IEnumerable<RtReadOnlyList<TValue>> Values => _groups.Values;
    }
}
