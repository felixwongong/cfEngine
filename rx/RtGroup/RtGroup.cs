using System;
using System.Collections.Generic;

namespace cfEngine.Rx
{
    public class RtGroup<TKey, TValue>: RtReadOnlyDictionary<TKey, RtReadOnlyList<TValue>>
    {
        private readonly ICollectionEvents<(int index, TValue item)> _source;
        private readonly Func<TValue, TKey> _keyFn;
        
        private readonly RtDictionary<TKey, RtList<TValue>> _groups = new();

        public RtGroup(RtReadOnlyList<TValue> source, Func<TValue, TKey> keyFn)
        {
            _source = source.Events;
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
        }

        public override IEnumerator<KeyValuePair<TKey, RtReadOnlyList<TValue>>> GetEnumerator()
        {
            foreach (var (key, group) in _groups)
            {
                yield return new KeyValuePair<TKey, RtReadOnlyList<TValue>>(key, group);
            }
        }

        public override int Count { get; }
        public override bool ContainsKey(TKey key)
        {
            throw new NotImplementedException();
        }

        public override bool TryGetValue(TKey key, out RtReadOnlyList<TValue> value)
        {
            throw new NotImplementedException();
        }

        public override RtReadOnlyList<TValue> this[TKey key] => throw new NotImplementedException();

        public override IEnumerable<TKey> Keys { get; }
        public override IEnumerable<RtReadOnlyList<TValue>> Values { get; }
    }
}