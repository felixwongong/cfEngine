using System;
using System.Collections.Generic;
using cfEngine.Logging;

namespace cfEngine.Rt
{
    public class RtSelectKeyDictionary<TOrigKey, TSelectKey, TValue>: RtMutatedDictionaryBase<TOrigKey, TValue, TSelectKey, TValue>
    {
        private readonly Func<TOrigKey, TSelectKey> _selectFn;

        public RtSelectKeyDictionary(RtReadOnlyDictionary<TOrigKey, TValue> source, Func<TOrigKey, TSelectKey> selectFn): base(source.Events)
        {
            _selectFn = selectFn;
            Mutated.EnsureCapacity(source.Count);
            foreach (var (origKey, value) in source)
            {
                Mutated[selectFn(origKey)] = value;
            }
        }

        protected override void OnSourceUpdate(KeyValuePair<TOrigKey, TValue> oldPair, KeyValuePair<TOrigKey, TValue> newPair)
        {
            var key = oldPair.Key;
            var oldValue = oldPair.Value;
            var newValue = newPair.Value;
            var selectedKey = _selectFn(key);
            
            if (TryGetValue(selectedKey, out var v) && v.Equals(oldValue))
            {
                Mutated[selectedKey] = newValue;
                CollectionEvents.OnUpdateRelay.Dispatch(
                    new KeyValuePair<TSelectKey, TValue>(selectedKey, v),
                    new KeyValuePair<TSelectKey, TValue>(selectedKey, newValue)
                    );
            }
            else
            {
                Log.LogException(new ArgumentException($"Invalid argument ({selectedKey.ToString()}, {v}, {newValue}), cannot update"), nameof(OnSourceUpdate));
            }
        }

        protected override void OnSourceRemove(KeyValuePair<TOrigKey, TValue> kvp)
        {
            var (key, value) = kvp;
            var selectedKey = _selectFn(key);
            
            if (TryGetValue(selectedKey, out var v) && v.Equals(value))
            {
                Mutated.Remove(selectedKey);
                CollectionEvents.OnRemoveRelay.Dispatch(new KeyValuePair<TSelectKey, TValue>(selectedKey, v));
            }
            else
            {
                Log.LogException(new ArgumentException($"Invalid argument ({selectedKey.ToString()}, {value.ToString()}), cannot remove"), nameof(OnSourceRemove));
            }
        }

        protected override void OnSourceAdd(KeyValuePair<TOrigKey, TValue> kvp)
        {
            var (key, value) = kvp;
            var selectedKey = _selectFn(key);
            if (Mutated.TryAdd(selectedKey, value))
            {
                CollectionEvents.OnAddRelay.Dispatch(new KeyValuePair<TSelectKey, TValue>(selectedKey, value));
            }
            else
            {
                Log.LogException(new ArgumentException($"Invalid argument ({selectedKey.ToString()}, {value.ToString()}), cannot add"), nameof(OnSourceAdd));
            }
        }
    }
}