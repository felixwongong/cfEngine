using System;
using cfEngine.Util;

namespace cfEngine.Rt
{
    public struct SubscriptionHandle
    {
        public WeakReference<Action> UnsubscribeAction;

        public void UnsubscribeIfNotNull()
        {
            if (UnsubscribeAction != null && UnsubscribeAction.TryGetTarget(out var action))
            {
                action?.Invoke();        
            }
        }
    }
    
    public interface ICollectionEvents<out T>
    {
        public event Action<T> OnAdd;
        public event Action<T> OnRemove;
        public event Action<T, T> OnUpdate;
        public event Action OnDispose;

        public SubscriptionHandle Subscribe(Action<T> onAdd = null, Action<T> onRemove = null, Action<T, T> onUpdate = null, Action onDispose = null);
        public void Unsubscribe(Action<T> onAdd = null, Action<T> onRemove = null, Action<T, T> onUpdate = null, Action onDispose = null);
    }
    
    public class CollectionEvents<T>: ICollectionEvents<T>, IDisposable
    {
        public readonly Relay<T> OnAddRelay = new();
        public readonly Relay<T> OnRemoveRelay = new();
        public readonly Relay<T, T> OnUpdateRelay = new();
        public readonly Relay OnDisposeRelay = new();

        public event Action<T> OnAdd
        {
            add => OnAddRelay.AddListener(value);
            remove => OnAddRelay.RemoveListener(value);
        } 
        
        public event Action<T> OnRemove
        {
            add => OnRemoveRelay.AddListener(value);
            remove => OnRemoveRelay.RemoveListener(value);
        } 
        
        public event Action<T, T> OnUpdate
        {
            add => OnUpdateRelay.AddListener(value);
            remove => OnUpdateRelay.RemoveListener(value);
        }

        public event Action OnDispose
        {
            add => OnDisposeRelay.AddListener(value);
            remove => OnDisposeRelay.RemoveListener(value);
        }

        public SubscriptionHandle Subscribe(Action<T> onAdd = null, Action<T> onRemove = null, Action<T, T> onUpdate = null, Action onDispose = null)
        {
            if (onAdd != null) {}
                OnAddRelay.AddListener(onAdd);

            if (onRemove != null)
                OnRemoveRelay.AddListener(onRemove);

            if (onUpdate != null)
                OnUpdateRelay.AddListener(onUpdate);

            if (onDispose != null)
                OnDisposeRelay.AddListener(onDispose);

            return new SubscriptionHandle()
            {
                UnsubscribeAction = new WeakReference<Action>(() => Unsubscribe(onAdd, onRemove, onUpdate, onDispose))
            };
        }

        public void Unsubscribe(Action<T> onAdd = null, Action<T> onRemove = null, Action<T, T> onUpdate = null, Action onDispose = null)
        {
            if (onAdd != null)
                OnAddRelay.RemoveListener(onAdd);

            if (onRemove != null)
                OnRemoveRelay.RemoveListener(onRemove);

            if (onUpdate != null)
                OnUpdateRelay.RemoveListener(onUpdate);

            if (onDispose != null)
                OnDisposeRelay.RemoveListener(onDispose);
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