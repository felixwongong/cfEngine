using System;
using System.Collections.Generic;

namespace cfEngine.Rt
{
    public class RtFilteredDictionary<TKey, TValue>: RtMutatedDictionaryBase<TKey, TValue, TKey, TValue>
    {
        private readonly Func<KeyValuePair<TKey, TValue>, bool> _filterFn;

        public RtFilteredDictionary(RtReadOnlyDictionary<TKey, TValue> source, Func<KeyValuePair<TKey, TValue>, bool> filterFn): base(source.Events, out var mutated)
        {
            _filterFn = filterFn;
            
            foreach (var kvp in source)
            {
                if(!filterFn(kvp)) continue;
                
                mutated.Add(kvp.Key, kvp.Value);
            }
        }

        protected override void _OnSourceUpdate(in Dictionary<TKey, TValue> mutated, KeyValuePair<TKey, TValue> oldPair,
            KeyValuePair<TKey, TValue> newPair)
        {
            bool canAdd, canRemove;

            canRemove = mutated.TryGetValue(oldPair.Key, out var oldValue) && oldValue.Equals(oldPair.Value);
            canAdd = _filterFn(newPair);

            if (canRemove && canAdd && oldPair.Key.Equals(newPair.Key))
            {
                mutated[oldPair.Key] = newPair.Value;
                CollectionEvents.OnUpdateRelay.Dispatch(oldPair, newPair);
            } else if (canAdd)
            {
                mutated[oldPair.Key] = newPair.Value;
                CollectionEvents.OnAddRelay.Dispatch(newPair);
            } else if (canRemove)
            {
                mutated.Remove(oldPair.Key);
                CollectionEvents.OnRemoveRelay.Dispatch(oldPair);
            }
        }

        protected override void _OnSourceRemove(in Dictionary<TKey, TValue> mutated, KeyValuePair<TKey, TValue> kvp)
        {
            if (mutated.TryGetValue(kvp.Key, out var oldValue) && oldValue.Equals(kvp.Value))
            {
                mutated.Remove(kvp.Key);
                CollectionEvents.OnRemoveRelay.Dispatch(kvp);
            }
        }

        protected override void _OnSourceAdd(in Dictionary<TKey, TValue> mutated, KeyValuePair<TKey, TValue> kvp)
        {
            if (_filterFn(kvp))
            {
                mutated[kvp.Key] = kvp.Value;
                CollectionEvents.OnAddRelay.Dispatch(kvp);
            }
        }
    }
}