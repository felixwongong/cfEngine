#if CF_REACTIVE_DEBUG

using System;
using System.Collections.Generic;
using System.Text;
using cfEngine.Util;

namespace cfEngine.Rt
{
    public interface IDebugMarked
    {
        public Guid __GetId();
        public string __GetDebugInfo();
    }
    public abstract partial class CollectionEventsBase: IDebugMarked
    {
        private Guid __Id;
        public void __SetCollectionId(Guid collectionId)
        {
            __Id = collectionId;
        }

        public Guid __GetId() => __Id;

        public string __GetDebugInfo() => string.Empty;
    }

    public abstract partial class Subscription: IDebugMarked
    {
        private Guid __Id = Guid.Empty;
        public Guid __GetId()
        {
            if (__Id.Equals(Guid.Empty))
            {
                __Id = Guid.NewGuid();
            }
            
            return __Id;
        }

        public abstract string __GetDebugInfo();
    }
    
    public abstract partial class RtCollection<TEventArgs>: IDebugMarked
    {
        private Guid __id = Guid.Empty;
        public Guid __GetId()
        {
            if (__id.Equals(Guid.Empty))
            {
                __id = Guid.NewGuid();
                CollectionEvents.__SetCollectionId(__id);
            }

            return __id;
        }

        public string __GetDebugInfo()
        {
            return string.Empty;
        }
    }

    public partial class SubscriptionBinding<TDelegate>
    {
        public override string __GetDebugInfo()
        {
            if (ListenerRef.TryGetTarget(out var listener))
            {
                return listener is Delegate d ? $"{d.Method.Name}: {d.Target.GetType().GetTypeName()}" : listener.GetType().GetTypeName();
            }
            else
            {
                return "No listener in subscription";
            }
        }
    }

    public partial class SubscriptionGroup: Subscription
    {
        public override string __GetDebugInfo()
        {
            var sb = new StringBuilder("SubscriptionGroup:");
            foreach (var subscription in _subscriptions)
            {
                sb.AppendLine(subscription.__GetDebugInfo());
            }

            return sb.ToString();
        }
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
        
        public void RecordSubscription(CollectionEventsBase events, WeakReference<Subscription> subscriptionRef)
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