using System.Collections.Generic;

namespace cfEngine.Rt
{
    public abstract class RtMutatedDictionaryBase<TSourceKey, TSourceValue, TKey, TValue>: RtReadOnlyDictionary<TKey, TValue>
    {
        private readonly ICollectionEvents<KeyValuePair<TSourceKey, TSourceValue>> _sourceEvents;
        protected readonly Dictionary<TKey, TValue> Mutated = new();

        protected RtMutatedDictionaryBase(ICollectionEvents<KeyValuePair<TSourceKey, TSourceValue>> sourceEvents)
        {
            _sourceEvents = sourceEvents;
            _sourceEvents.Subscribe(OnSourceAdd, OnSourceRemove, OnSourceUpdate, Dispose);
        }

        protected abstract void OnSourceUpdate(KeyValuePair<TSourceKey, TSourceValue> oldPair, KeyValuePair<TSourceKey, TSourceValue> newPair);

        protected abstract void OnSourceRemove(KeyValuePair<TSourceKey, TSourceValue> kvp);

        protected abstract void OnSourceAdd(KeyValuePair<TSourceKey, TSourceValue> kvp);

        #region IReadOnlyDictionary Implementation

        public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return Mutated.GetEnumerator();
        }

        public override int Count => Mutated.Count;
        public override bool ContainsKey(TKey key)
        {
            return Mutated.ContainsKey(key);
        }

        public override bool TryGetValue(TKey key, out TValue value)
        {
            return Mutated.TryGetValue(key, out value);
        }

        public override TValue this[TKey key] => Mutated[key];

        public override IEnumerable<TKey> Keys => Mutated.Keys;
        public override IEnumerable<TValue> Values => Mutated.Values;

        #endregion

        public override void Dispose()
        {
            base.Dispose();
            
            _sourceEvents.Unsubscribe(OnSourceAdd, OnSourceRemove, OnSourceUpdate, Dispose);
            Mutated.Clear();
        }
    }
}