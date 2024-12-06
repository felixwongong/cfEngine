using System;
using cfEngine.Util;

namespace cfEngine.Rt
{
    public partial class CollectionEventsBase
    {
    }
    
    public interface ICollectionEvents<out T>
    {
        public Subscription SubscribeOnAdd(Action<T> onAdd);
        public Subscription SubscribeOnRemove(Action<T> onRemove);
        public Subscription SubscribeOnUpdate(Action<T, T> onUpdate);
        public Subscription SubscribeOnDispose(Action onDispose);
        public Subscription Subscribe(Action<T> onAdd = null, Action<T> onRemove = null, Action<T, T> onUpdate = null, Action onDispose = null);
    }
    
    public partial class CollectionEvents<T>: CollectionEventsBase, ICollectionEvents<T>
    {
        public readonly Relay<T> OnAddRelay = new();
        public readonly Relay<T> OnRemoveRelay = new();
        public readonly Relay<T, T> OnUpdateRelay = new();
        public readonly Relay OnDisposeRelay = new();

        public Subscription SubscribeOnAdd(Action<T> onAdd)
        {
            var sub = OnAddRelay.AddListener(onAdd);
#if UNITY_EDITOR
            _RtDebug.Instance.RecordSubscription(this, sub);
#endif
            return sub;
        }
        
        public Subscription SubscribeOnRemove(Action<T> onRemove)
        {
            var sub = OnRemoveRelay.AddListener(onRemove);
#if UNITY_EDITOR
            _RtDebug.Instance.RecordSubscription(this, sub);
#endif
            return sub;
        }
        
        public Subscription SubscribeOnUpdate(Action<T, T> onUpdate)
        {
            var sub = OnUpdateRelay.AddListener(onUpdate);
#if UNITY_EDITOR
            _RtDebug.Instance.RecordSubscription(this, sub);
#endif
            return sub;
        }

        public Subscription SubscribeOnDispose(Action onDispose)
        {
            var sub = OnDisposeRelay.AddListener(onDispose);
#if UNITY_EDITOR
            _RtDebug.Instance.RecordSubscription(this, sub);
#endif
            return sub;
        }
        
        public Subscription Subscribe(Action<T> onAdd = null, Action<T> onRemove = null, Action<T, T> onUpdate = null, Action onDispose = null)
        {
            var handle = new SubscriptionGroup();
            if (onAdd != null)
                handle.Add(SubscribeOnAdd(onAdd));

            if (onRemove != null)
                handle.Add(SubscribeOnRemove(onRemove));

            if (onUpdate != null)
                handle.Add(SubscribeOnUpdate(onUpdate));

            if (onDispose != null) 
                handle.Add(SubscribeOnDispose(onDispose));

            return handle;
        }
    }
}