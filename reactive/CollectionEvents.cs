using System;

namespace cfEngine.Rt
{
    public partial interface ICollectionEvents<out T>
    {
        public Subscription SubscribeOnAdd(Action<T> onAdd);
        public Subscription SubscribeOnRemove(Action<T> onRemove);
        public Subscription SubscribeOnUpdate(Action<T, T> onUpdate);
        public Subscription SubscribeOnDispose(Action onDispose);
        public void Dispose();
    }
    
    public partial class CollectionEvents<T>: ICollectionEvents<T>
    {
        public readonly Relay<T> OnAddRelay = new();
        public readonly Relay<T> OnRemoveRelay = new();
        public readonly Relay<T, T> OnUpdateRelay = new();
        public readonly Relay OnDisposeRelay = new();

        public Subscription SubscribeOnAdd(Action<T> onAdd)
        {
            var sub = OnAddRelay.AddListener(onAdd);
#if CF_REACTIVE_DEBUG
            _RtDebug.Instance.RecordSubscription<CollectionEvents<T>, T>(this, new WeakReference<Subscription>(sub));
#endif
            return sub;
        }
        
        public Subscription SubscribeOnRemove(Action<T> onRemove)
        {
            var sub = OnRemoveRelay.AddListener(onRemove);
#if CF_REACTIVE_DEBUG
            _RtDebug.Instance.RecordSubscription<CollectionEvents<T>, T>(this, new WeakReference<Subscription>(sub));
#endif
            return sub;
        }
        
        public Subscription SubscribeOnUpdate(Action<T, T> onUpdate)
        {
            var sub = OnUpdateRelay.AddListener(onUpdate);
#if CF_REACTIVE_DEBUG
            _RtDebug.Instance.RecordSubscription<CollectionEvents<T>, T>(this, new WeakReference<Subscription>(sub));
#endif
            return sub;
        }

        public Subscription SubscribeOnDispose(Action onDispose)
        {
            var sub = OnDisposeRelay.AddListener(onDispose);
#if CF_REACTIVE_DEBUG
            _RtDebug.Instance.RecordSubscription<CollectionEvents<T>, T>(this, new WeakReference<Subscription>(sub));
#endif
            return sub;
        }
        
        public void Dispose()
        {
            OnAddRelay.RemoveAll();
            OnRemoveRelay.RemoveAll();
            OnUpdateRelay.RemoveAll();
            OnDisposeRelay.RemoveAll();
        }
    }
}