using System;
using System.Collections.Generic;

namespace cfEngine.Rt
{
    public class RtFilteredDictionary<TKey, TValue>: RtMutatedDictionaryBase<TKey, TValue, TKey, TValue>
    {
        private readonly Func<KeyValuePair<TKey, TValue>, bool> _filterFn;

        public RtFilteredDictionary(RtReadOnlyDictionary<TKey, TValue> source, Func<KeyValuePair<TKey, TValue>, bool> filterFn): base(source.Events)
        {
            _filterFn = filterFn;
            
            foreach (var kvp in source)
            {
                if(!filterFn(kvp)) continue;
                
                Mutated.Add(kvp.Key, kvp.Value);
            }
        }

        protected override void OnSourceUpdate(KeyValuePair<TKey, TValue> oldPair, KeyValuePair<TKey, TValue> newPair)
        {
            bool canAdd, canRemove;

            canRemove = Mutated.TryGetValue(oldPair.Key, out var oldValue) && oldValue.Equals(oldPair.Value);
            canAdd = _filterFn(newPair);

            if (canRemove && canAdd && oldPair.Key.Equals(newPair.Key))
            {
                Mutated[oldPair.Key] = newPair.Value;
                CollectionEvents.OnUpdateRelay.Dispatch(oldPair, newPair);
            } else if (canAdd)
            {
                Mutated[oldPair.Key] = newPair.Value;
                CollectionEvents.OnAddRelay.Dispatch(newPair);
            } else if (canRemove)
            {
                Mutated.Remove(oldPair.Key);
                CollectionEvents.OnRemoveRelay.Dispatch(oldPair);
            }
        }

        protected override void OnSourceRemove(KeyValuePair<TKey, TValue> kvp)
        {
            if (Mutated.TryGetValue(kvp.Key, out var oldValue) && oldValue.Equals(kvp.Value))
            {
                Mutated.Remove(kvp.Key);
                CollectionEvents.OnRemoveRelay.Dispatch(kvp);
            }
        }

        protected override void OnSourceAdd(KeyValuePair<TKey, TValue> kvp)
        {
            if (_filterFn(kvp))
            {
                Mutated[kvp.Key] = kvp.Value;
                CollectionEvents.OnAddRelay.Dispatch(kvp);
            }
        }
    }
}