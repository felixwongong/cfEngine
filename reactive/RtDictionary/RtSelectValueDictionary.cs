using System;
using System.Collections.Generic;
using cfEngine.Logging;

namespace cfEngine.Rt
{
    /// <summary>
    /// Represents a dictionary that selects values based on a provided function.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TOrigValue">The type of the original values in the source dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the selected values in the mutated dictionary.</typeparam>
    public class RtSelectValueDictionary<TKey, TOrigValue, TValue> : RtMutatedDictionaryBase<TKey, TOrigValue, TKey, TValue>
    {
        private readonly Func<TOrigValue, TValue> _selectFn;

        /// <summary>
        /// Initializes a new instance of the <see cref="RtSelectValueDictionary{TKey, TOrigValue, TValue}"/> class.
        /// </summary>
        /// <param name="source">The source read-only dictionary.</param>
        /// <param name="selectFn">The function to select values.</param>
        public RtSelectValueDictionary(RtReadOnlyDictionary<TKey, TOrigValue> source, Func<TOrigValue, TValue> selectFn)
            : base(source.Events)
        {
            _selectFn = selectFn ?? throw new ArgumentNullException(nameof(selectFn));
            foreach (var (key, origValue) in source)
            {
                _mutated[key] = _selectFn(origValue);
            }
        }

        #region OnSourceCollectionUpdate

        protected override void _OnSourceUpdate(in Dictionary<TKey, TValue> mutated,
            KeyValuePair<TKey, TOrigValue> oldPair,
            KeyValuePair<TKey, TOrigValue> newPair)
        {
            var key = oldPair.Key;
            if (!mutated.TryGetValue(key, out var oldSelected))
            {
                Log.LogException(new ArgumentException($"Invalid argument ({key}, {oldPair.Value}, {newPair.Value}), cannot update"), nameof(_OnSourceUpdate));
                return;
            }

            var newSelected = _selectFn(newPair.Value);
            mutated[key] = newSelected;
            CollectionEvents.OnUpdateRelay.Dispatch(
                new KeyValuePair<TKey, TValue>(key, oldSelected),
                new KeyValuePair<TKey, TValue>(key, newSelected)
            );
        }

        protected override void _OnSourceRemove(in Dictionary<TKey, TValue> mutated, KeyValuePair<TKey, TOrigValue> kvp)
        {
            var key = kvp.Key;
            if (!mutated.TryGetValue(key, out var selectedValue))
            {
                Log.LogException(new ArgumentException($"Invalid argument ({key}, {kvp.Value}), cannot remove"), nameof(_OnSourceRemove));
                return;
            }

            mutated.Remove(key);
            CollectionEvents.OnRemoveRelay.Dispatch(new KeyValuePair<TKey, TValue>(key, selectedValue));
        }

        protected override void _OnSourceAdd(in Dictionary<TKey, TValue> mutated, KeyValuePair<TKey, TOrigValue> kvp)
        {
            var key = kvp.Key;
            var selectedValue = _selectFn(kvp.Value);

            if (!mutated.TryAdd(key, selectedValue))
            {
                Log.LogException(new ArgumentException($"Invalid argument ({key}, {selectedValue}), cannot add"), nameof(_OnSourceAdd));
                return;
            }

            CollectionEvents.OnAddRelay.Dispatch(new KeyValuePair<TKey, TValue>(key, selectedValue));
        }

        #endregion
    }
}
