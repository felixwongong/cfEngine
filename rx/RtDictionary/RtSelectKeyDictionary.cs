using System;
using System.Collections.Generic;
using cfEngine.Logging;

namespace cfEngine.Rt
{
    public class RtSelectKeyDictionary<TOrigKey, TSelectKey, TValue>: RtMutatedDictionaryBase<TOrigKey, TValue, TSelectKey, TValue>
    {
        private readonly Func<TOrigKey, TSelectKey> _selectFn;

        public RtSelectKeyDictionary(RtReadOnlyDictionary<TOrigKey, TValue> source, Func<TOrigKey, TSelectKey> selectFn): base(source.Events, out var mutated)
        {
            _selectFn = selectFn;
            mutated.EnsureCapacity(source.Count);
            foreach (var (origKey, value) in source)
            {
                mutated[selectFn(origKey)] = value;
            }
        }

        protected override void _OnSourceUpdate(in Dictionary<TSelectKey, TValue> mutated,
            KeyValuePair<TOrigKey, TValue> oldPair,
            KeyValuePair<TOrigKey, TValue> newPair)
        {
            var key = oldPair.Key;
            var oldValue = oldPair.Value;
            var newValue = newPair.Value;
            var selectedKey = _selectFn(key);
            
            if (TryGetValue(selectedKey, out var v) && v.Equals(oldValue))
            {
                mutated[selectedKey] = newValue;
                CollectionEvents.OnUpdateRelay.Dispatch(
                    new KeyValuePair<TSelectKey, TValue>(selectedKey, v),
                    new KeyValuePair<TSelectKey, TValue>(selectedKey, newValue)
                    );
            }
            else
            {
                Log.LogException(new ArgumentException($"Invalid argument ({selectedKey.ToString()}, {v}, {newValue}), cannot update"), nameof(_OnSourceUpdate));
            }
        }

        protected override void _OnSourceRemove(in Dictionary<TSelectKey, TValue> mutated,
            KeyValuePair<TOrigKey, TValue> kvp)
        {
            var (key, value) = kvp;
            var selectedKey = _selectFn(key);
            
            if (TryGetValue(selectedKey, out var v) && v.Equals(value))
            {
                mutated.Remove(selectedKey);
                CollectionEvents.OnRemoveRelay.Dispatch(new KeyValuePair<TSelectKey, TValue>(selectedKey, v));
            }
            else
            {
                Log.LogException(new ArgumentException($"Invalid argument ({selectedKey.ToString()}, {value.ToString()}), cannot remove"), nameof(_OnSourceRemove));
            }
        }

        protected override void _OnSourceAdd(in Dictionary<TSelectKey, TValue> mutated, KeyValuePair<TOrigKey, TValue> kvp)
        {
            var (key, value) = kvp;
            var selectedKey = _selectFn(key);
            if (mutated.TryAdd(selectedKey, value))
            {
                CollectionEvents.OnAddRelay.Dispatch(new KeyValuePair<TSelectKey, TValue>(selectedKey, value));
            }
            else
            {
                Log.LogException(new ArgumentException($"Invalid argument ({selectedKey.ToString()}, {value.ToString()}), cannot add"), nameof(_OnSourceAdd));
            }
        }
    }
}