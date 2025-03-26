using System;
using System.Collections.Generic;
using cfEngine.Logging;

namespace cfEngine.Rx
{
    /// <summary>
    /// Represents a dictionary that selects keys based on a provided function.
    /// </summary>
    /// <typeparam name="TOrigKey">The type of the original keys in the source dictionary.</typeparam>
    /// <typeparam name="TSelectKey">The type of the selected keys in the mutated dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    public class RtSelectKeyDictionary<TOrigKey, TSelectKey, TValue> : RtMutatedDictionaryBase<TOrigKey, TValue, TSelectKey, TValue>
    {
        private readonly Func<TOrigKey, TSelectKey> _selectFn;

        /// <summary>
        /// Initializes a new instance of the <see cref="RtSelectKeyDictionary{TOrigKey, TSelectKey, TValue}"/> class.
        /// </summary>
        /// <param name="source">The source read-only dictionary.</param>
        /// <param name="selectFn">The function to select keys.</param>
        public RtSelectKeyDictionary(RtReadOnlyDictionary<TOrigKey, TValue> source, Func<TOrigKey, TSelectKey> selectFn) : base(source.Events)
        {
            _selectFn = selectFn ?? throw new ArgumentNullException(nameof(selectFn));
            _mutated.EnsureCapacity(source.Count);
            foreach (var (origKey, value) in source)
            {
                _mutated[selectFn(origKey)] = value;
            }
        }

        protected override void _OnSourceUpdate(in Dictionary<TSelectKey, TValue> mutated,
            KeyValuePair<TOrigKey, TValue> oldPair,
            KeyValuePair<TOrigKey, TValue> newPair)
        {
            var selectedKey = _selectFn(oldPair.Key);

            if (mutated.TryGetValue(selectedKey, out var v) && v.Equals(oldPair.Value))
            {
                mutated[selectedKey] = newPair.Value;
                CollectionEvents.OnUpdateRelay.Dispatch(
                    new KeyValuePair<TSelectKey, TValue>(selectedKey, v),
                    new KeyValuePair<TSelectKey, TValue>(selectedKey, newPair.Value)
                );
            }
            else
            {
                Log.LogException(new ArgumentException($"Invalid argument ({selectedKey}, {v}, {newPair.Value}), cannot update"), nameof(_OnSourceUpdate));
            }
        }

        protected override void _OnSourceRemove(in Dictionary<TSelectKey, TValue> mutated,
            KeyValuePair<TOrigKey, TValue> kvp)
        {
            var selectedKey = _selectFn(kvp.Key);

            if (mutated.TryGetValue(selectedKey, out var v) && v.Equals(kvp.Value))
            {
                mutated.Remove(selectedKey);
                CollectionEvents.OnRemoveRelay.Dispatch(new KeyValuePair<TSelectKey, TValue>(selectedKey, v));
            }
            else
            {
                Log.LogException(new ArgumentException($"Invalid argument ({selectedKey}, {kvp.Value}), cannot remove"), nameof(_OnSourceRemove));
            }
        }

        protected override void _OnSourceAdd(in Dictionary<TSelectKey, TValue> mutated, KeyValuePair<TOrigKey, TValue> kvp)
        {
            var selectedKey = _selectFn(kvp.Key);
            if (mutated.TryAdd(selectedKey, kvp.Value))
            {
                CollectionEvents.OnAddRelay.Dispatch(new KeyValuePair<TSelectKey, TValue>(selectedKey, kvp.Value));
            }
            else
            {
                Log.LogException(new ArgumentException($"Invalid argument ({selectedKey}, {kvp.Value}), cannot add"), nameof(_OnSourceAdd));
            }
        }
    }
}
