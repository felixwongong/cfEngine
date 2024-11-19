using System;
using System.Collections.Generic;

namespace cfEngine.Rt
{
    /// <summary>
    /// Represents a dictionary that filters its elements based on a provided function.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    public class RtFilteredDictionary<TKey, TValue> : RtMutatedDictionaryBase<TKey, TValue, TKey, TValue>
    {
        private readonly Func<KeyValuePair<TKey, TValue>, bool> _filterFn;

        /// <summary>
        /// Initializes a new instance of the <see cref="RtFilteredDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="source">The source dictionary.</param>
        /// <param name="filterFn">The function to filter elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="filterFn"/> is null.</exception>
        public RtFilteredDictionary(RtReadOnlyDictionary<TKey, TValue> source, Func<KeyValuePair<TKey, TValue>, bool> filterFn) : base(source.Events, out var mutated)
        {
            _filterFn = filterFn ?? throw new ArgumentNullException(nameof(filterFn));

            foreach (var kvp in source)
            {
                if (!filterFn(kvp)) continue;

                mutated.Add(kvp.Key, kvp.Value);
            }
        }

        protected override void _OnSourceUpdate(in Dictionary<TKey, TValue> mutated, KeyValuePair<TKey, TValue> oldPair, KeyValuePair<TKey, TValue> newPair)
        {
            bool canRemove = mutated.TryGetValue(oldPair.Key, out var oldValue) && oldValue.Equals(oldPair.Value);
            bool canAdd = _filterFn(newPair);

            if (canRemove && canAdd && oldPair.Key.Equals(newPair.Key))
            {
                mutated[oldPair.Key] = newPair.Value;
                CollectionEvents.OnUpdateRelay.Dispatch(oldPair, newPair);
            }
            else if (canAdd)
            {
                mutated[oldPair.Key] = newPair.Value;
                CollectionEvents.OnAddRelay.Dispatch(newPair);
            }
            else if (canRemove)
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
