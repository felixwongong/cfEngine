using System;
using System.Collections.Generic;
using cfEngine.Util;

namespace cfEngine.Rx
{
    /// <summary>
    /// Represents a base class for dictionaries that support mutation and event dispatching.
    /// </summary>
    /// <typeparam name="TSourceKey">The type of keys in the source dictionary.</typeparam>
    /// <typeparam name="TSourceValue">The type of values in the source dictionary.</typeparam>
    /// <typeparam name="TKey">The type of keys in the mutated dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the mutated dictionary.</typeparam>
    public abstract class RxMutatedDictionaryBase<TSourceKey, TSourceValue, TKey, TValue> : RxReadOnlyDictionary<TKey, TValue>
    {
        protected readonly Dictionary<TKey, TValue> _mutated = new();

        Subscription _sourceChangeSubscription;
        
        protected RxMutatedDictionaryBase(ICollectionEvents<KeyValuePair<TSourceKey, TSourceValue>> sourceEvents): base()
        {
            _sourceChangeSubscription = sourceEvents.Subscribe(OnSourceAdd, OnSourceRemove, OnSourceUpdate, Dispose);

#if CF_REACTIVE_DEBUG
            __SetSourceCollectionId(sourceEvents);
#endif
        }
        
        public override void Dispose()
        {
            base.Dispose();
            _sourceChangeSubscription.Unsubscribe();
            
            foreach (var (key, value) in _mutated)
            {
                if(key is IDisposable disposableKey) disposableKey.Dispose();
                if(value is IDisposable disposableValue) disposableValue.Dispose();
            }
            _mutated.Clear();
        }

        private void OnSourceUpdate(KeyValuePair<TSourceKey, TSourceValue> oldPair, KeyValuePair<TSourceKey, TSourceValue> newPair)
        {
            _OnSourceUpdate(_mutated, oldPair, newPair);
        }

        private void OnSourceRemove(KeyValuePair<TSourceKey, TSourceValue> kvp)
        {
            _OnSourceRemove(_mutated, kvp);
        }

        private void OnSourceAdd(KeyValuePair<TSourceKey, TSourceValue> kvp)
        {
            _OnSourceAdd(_mutated, kvp);
        }

        protected abstract void _OnSourceUpdate(in Dictionary<TKey, TValue> mutated, KeyValuePair<TSourceKey, TSourceValue> oldPair, KeyValuePair<TSourceKey, TSourceValue> newPair);

        protected abstract void _OnSourceRemove(in Dictionary<TKey, TValue> mutated, KeyValuePair<TSourceKey, TSourceValue> kvp);

        protected abstract void _OnSourceAdd(in Dictionary<TKey, TValue> mutated, KeyValuePair<TSourceKey, TSourceValue> kvp);

        #region IReadOnlyDictionary Implementation

        public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _mutated.GetEnumerator();
        }

        public override int Count => _mutated.Count;
        public override bool ContainsKey(TKey key)
        {
            return _mutated.ContainsKey(key);
        }

        public override bool TryGetValue(TKey key, out TValue value)
        {
            return _mutated.TryGetValue(key, out value);
        }

        public override TValue this[TKey key] => _mutated[key];

        public override IEnumerable<TKey> Keys => _mutated.Keys;
        public override IEnumerable<TValue> Values => _mutated.Values;

        #endregion
    }
}
