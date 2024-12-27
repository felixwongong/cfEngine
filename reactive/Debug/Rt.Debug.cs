#if CF_REACTIVE_DEBUG

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace cfEngine.Rt
{
    public interface IMarkedDebug
    {
        public Guid __GetId();
        public string __GetDebugTitle();
    }

    public class _RtDebug
    {
        private static _RtDebug _instance;
        public static _RtDebug Instance => _instance ??= new _RtDebug();
        
        private Dictionary<Guid, WeakReference<ICollectionDebug>> _collections = new();      //Dictionary<CollectionId, IEnumerable<object>>
        public IReadOnlyDictionary<Guid, WeakReference<ICollectionDebug>> Collections => _collections;

        private Dictionary<Guid, List<Guid>> _collectionMutatedReferenceMap = new();

        private Dictionary<Guid, Dictionary<Guid, WeakReference<Subscription>>> _collectionSubs = new();      //Dictionary<CollectionId, Dictionary<SubscriptionId, Subscription>>
        public IReadOnlyDictionary<Guid, Dictionary<Guid, WeakReference<Subscription>>> CollectionSubs => _collectionSubs;

        public void RecordCollection<TEventArgs>(RtCollection<TEventArgs> collection)
        {
            _collections.Add(collection.__GetId(), new WeakReference<ICollectionDebug>(collection));
        }

        public void RecordMutatedReference(Guid sourceCollectionId, Guid mutatedCollectionId)
        {
            if(!_collectionMutatedReferenceMap.TryGetValue(sourceCollectionId, out var mutatedCollectionIds))
            {
                mutatedCollectionIds = new();
                _collectionMutatedReferenceMap.Add(sourceCollectionId, mutatedCollectionIds);
            }
            
            mutatedCollectionIds.Add(mutatedCollectionId);
        }
        
        public bool TryGetMutatedReferences(Guid sourceCollectionId, out List<Guid> mutatedCollectionIds)
        {
            return _collectionMutatedReferenceMap.TryGetValue(sourceCollectionId, out mutatedCollectionIds);
        }

        public void RemoveCollectionRecord<TEventArgs>(RtCollection<TEventArgs> collection)
        {
            var id = collection.__GetId();
            _collections.Remove(id);
            _collectionMutatedReferenceMap.Remove(id);
            _collectionSubs.Remove(id);
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

        public IList GetRootCollectionIds()
        {
            return _collections.Where(kvp =>
                kvp.Value.TryGetTarget(out var collectionDebug) && collectionDebug.__IsRoot())
                .Select(kvp => kvp.Key).ToList();
        }
    }

}

#endif