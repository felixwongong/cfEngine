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
        public readonly Relay<T> OnAddRelay;
        public readonly Relay<T> OnRemoveRelay;
        public readonly Relay<T, T> OnUpdateRelay;
        public readonly Relay OnDisposeRelay;

#pragma warning disable 0414
        private object _o;
#pragma warning restore 0414
        
        public CollectionEvents(object owner)
        {
            _o = owner;

            OnAddRelay = new Relay<T>(this);
            OnRemoveRelay = new Relay<T>(this);
            OnUpdateRelay = new Relay<T, T>(this);
            OnDisposeRelay = new Relay(this);
        }

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