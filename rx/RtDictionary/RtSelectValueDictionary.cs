using System;
using System.Collections.Generic;
using cfEngine.Logging;

namespace cfEngine.Rt
{
    public class RtSelectValueDictionary<TKey, TOrigValue, TValue>: RtMutatedDictionaryBase<TKey, TOrigValue, TKey, TValue>
    {
        private readonly Func<TOrigValue,TValue> _selectFn;

        public RtSelectValueDictionary(RtReadOnlyDictionary<TKey, TOrigValue> source, Func<TOrigValue, TValue> selectFn): base(source.Events, out var mutated)
        {
            _selectFn = selectFn;
            foreach (var (key, origValue) in source)
            {
                mutated[key] = _selectFn(origValue);
            }
        }

        #region OnSourceCollectionUpdate

        protected override void _OnSourceUpdate(in Dictionary<TKey, TValue> mutated,
            KeyValuePair<TKey, TOrigValue> oldPair,
            KeyValuePair<TKey, TOrigValue> newPair)
        {
            var key = oldPair.Key;
            var oldValue = oldPair.Value;
            var newValue = newPair.Value;
            
            if (!mutated.TryGetValue(key, out var oldSelected))
            {
                Log.LogException(new ArgumentException($"Invalid argument ({key}, {oldValue.ToString()}, {newValue.ToString()}), cannot update"), nameof(_OnSourceUpdate));
                return;
            }

            var newSelected = _selectFn(newValue);
            mutated[key] = newSelected; 
            CollectionEvents.OnUpdateRelay.Dispatch(
                new KeyValuePair<TKey, TValue>(key, oldSelected),
                new KeyValuePair<TKey, TValue>(key, newSelected)
                );
        }

        protected override void _OnSourceRemove(in Dictionary<TKey, TValue> mutated, KeyValuePair<TKey, TOrigValue> kvp)
        {
            var (key, value) = kvp;
            if (!mutated.TryGetValue(key, out var selectedValue))
            {
                Log.LogException(new ArgumentException($"Invalid argument ({key.ToString()}, {value.ToString()}), cannot remove"), nameof(_OnSourceRemove));
                return;
            }

            mutated.Remove(key);
            CollectionEvents.OnRemoveRelay.Dispatch(new (key, selectedValue));
        }

        protected override void _OnSourceAdd(in Dictionary<TKey, TValue> mutated, KeyValuePair<TKey, TOrigValue> kvp)
        {
            var (key, value) = kvp;
            var selectedValue = _selectFn(value);

            if (!mutated.TryAdd(key, selectedValue))
            {
                Log.LogException(new ArgumentException($"Invalid argument ({key.ToString()}, {selectedValue.ToString()}), cannot add"), nameof(_OnSourceAdd));
                return;
            }
            
            CollectionEvents.OnAddRelay.Dispatch(new KeyValuePair<TKey, TValue>(key, selectedValue));
        }

        #endregion
    }
}