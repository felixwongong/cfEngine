using System;
using cfEngine.Util;

namespace cfEngine.Rt
{
    public interface ICollectionEvents<out T>
    {
        public Subscription Subscribe(Action<T> onAdd = null, Action<T> onRemove = null, Action<T, T> onUpdate = null, Action onDispose = null);
    }
    
    public class CollectionEvents<T>: ICollectionEvents<T>
    {
        public readonly Relay<T> OnAddRelay = new();
        public readonly Relay<T> OnRemoveRelay = new();
        public readonly Relay<T, T> OnUpdateRelay = new();
        public readonly Relay OnDisposeRelay = new();

        public Subscription Subscribe(Action<T> onAdd = null, Action<T> onRemove = null, Action<T, T> onUpdate = null, Action onDispose = null)
        {
            var handle = new SubscriptionGroup();
            if (onAdd != null)
                handle.Add(OnAddRelay.AddListener(onAdd));

            if (onRemove != null)
                handle.Add(OnRemoveRelay.AddListener(onRemove));

            if (onUpdate != null)
                handle.Add(OnUpdateRelay.AddListener(onUpdate));

            if (onDispose != null) 
                handle.Add(OnDisposeRelay.AddListener(onDispose));

            return handle;
        }
    }
}