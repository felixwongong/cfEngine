using System.Collections.Generic;

namespace cfEngine.Rt
{
    public abstract class RtMutatedDictionaryBase<TSourceKey, TSourceValue, TKey, TValue>: RtReadOnlyDictionary<TKey, TValue>
    {
        private readonly ICollectionEvents<KeyValuePair<TSourceKey, TSourceValue>> _sourceEvents;

        protected RtMutatedDictionaryBase(ICollectionEvents<KeyValuePair<TSourceKey, TSourceValue>> sourceEvents)
        {
            _sourceEvents = sourceEvents;
            _sourceEvents.Subscribe(OnSourceAdd, OnSourceRemove, OnSourceUpdate, Dispose);
        }

        protected abstract void OnSourceUpdate(KeyValuePair<TSourceKey, TSourceValue> arg1, KeyValuePair<TSourceKey, TSourceValue> arg2);

        protected abstract void OnSourceRemove(KeyValuePair<TSourceKey, TSourceValue> obj);

        protected abstract void OnSourceAdd(KeyValuePair<TSourceKey, TSourceValue> obj);

        public override void Dispose()
        {
            base.Dispose();
            _sourceEvents.Unsubscribe(OnSourceAdd, OnSourceRemove, OnSourceUpdate, Dispose);
        }
    }
}