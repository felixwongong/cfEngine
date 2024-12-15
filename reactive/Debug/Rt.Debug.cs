#if CF_REACTIVE_DEBUG

using System;
using System.Collections.Generic;

namespace cfEngine.Rt
{
    public interface IMarkedDebug
    {
        public Guid __GetId();
        public string __GetDebugInfo();
    }
    public class _RtDebug
    {
        private static _RtDebug _instance;
        public static _RtDebug Instance => _instance ??= new _RtDebug();
        
        private Dictionary<Guid, WeakReference<object>> _collections = new();      //Dictionary<CollectionId, IEnumerable<object>>
        public IReadOnlyDictionary<Guid, WeakReference<object>> Collections => _collections;

        private Dictionary<Guid, Dictionary<Guid, WeakReference<Subscription>>> _collectionSubs = new();      //Dictionary<CollectionId, Dictionary<SubscriptionId, Subscription>>
        public IReadOnlyDictionary<Guid, Dictionary<Guid, WeakReference<Subscription>>> CollectionSubs => _collectionSubs;

        public void RecordCollection<TEventArgs>(RtCollection<TEventArgs> collection)
        {
            _collections.Add(collection.__GetId(), new WeakReference<object>(collection));
        }
        
        public void RecordSubscription<T, TArg>(T events, WeakReference<Subscription> subscriptionRef) where T: ICollectionEvents<TArg>
        {
            if (!_collectionSubs.TryGetValue(events.__GetId(), out var subscriptions))
            {
                subscriptions = new();
                _collectionSubs.Add(events.__GetId(), subscriptions);
            }

            if (subscriptionRef.TryGetTarget(out var sub))
            {
                subscriptions.Add(sub.__GetId(), subscriptionRef);
            }
        }

        public IReadOnlyDictionary<Guid, WeakReference<Subscription>> GetCollectionSubs(Guid collectionId)
        {
            return _collectionSubs.GetValueOrDefault(collectionId);
        }
    }

}

#endif