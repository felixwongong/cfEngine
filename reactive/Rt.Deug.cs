using System;
using System.Collections.Generic;
using System.Linq;

namespace cfEngine.Rt
{
    public partial class CollectionEventsBase
    {
#if UNITY_EDITOR
        public Guid Id { get; private set; }
        public void SetCollectionId(Guid collectionId)
        {
            Id = collectionId;
        }
#endif
    }

    public abstract partial class Subscription
    {
#if UNITY_EDITOR
        private static int _uniqueId;
        private static int getId => unchecked(_uniqueId++);
        public int Id = getId;
#endif
    }

    public abstract partial class RtReadOnlyDictionary<TKey, TValue>
    {
#if UNITY_EDITOR
        private Guid id = Guid.Empty;

        public Guid Id
        {
            get
            {
                if (id == Guid.Empty)
                {
                    id = Guid.NewGuid();
                    CollectionEvents.SetCollectionId(id);
                }

                return id;
            }
        }
#endif
    }

    public abstract partial class RtReadOnlyList<T>
    {
#if UNITY_EDITOR
        private Guid id = Guid.Empty;

        public Guid Id
        {
            get
            {
                if (id == Guid.Empty)
                {
                    id = Guid.NewGuid();
                    CollectionEvents.SetCollectionId(id);
                }

                return id;
            }
        }
#endif
    }

    public class _RtDebug
    {
        private static _RtDebug _instance;
        public static _RtDebug Instance => _instance ??= new _RtDebug();
        
        private Dictionary<Guid, WeakReference<object>> _collections = new();      //Dictionary<CollectionId, IEnumerable<object>>
        public IReadOnlyDictionary<Guid, WeakReference<object>> Collections => _collections;

        private Dictionary<Guid, Dictionary<int, WeakReference<Subscription>>> _collectionSubs = new();      //Dictionary<CollectionId, Dictionary<SubscriptionId, Subscription>>
        public IReadOnlyDictionary<Guid, Dictionary<int, WeakReference<Subscription>>> CollectionSubs => _collectionSubs;

        public void RecordCollection<TKey, TValue>(RtReadOnlyDictionary<TKey, TValue> collection)
        {
            _collections.Add(collection.Id, new WeakReference<object>(collection));
        }
        
        public void RecordCollection<T>(RtReadOnlyList<T> collection)
        {
            _collections.Add(collection.Id, new WeakReference<object>(collection));
        }
        
        public void RecordSubscription(CollectionEventsBase events, Subscription subscription)
        {
            if (!_collectionSubs.TryGetValue(events.Id, out var subscriptions))
            {
                subscriptions = new Dictionary<int, WeakReference<Subscription>>();
                _collectionSubs.Add(events.Id, subscriptions);
            }

            subscriptions.Add(subscription.Id, new WeakReference<Subscription>(subscription));
        }
    }

}