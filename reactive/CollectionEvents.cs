using System;
using System.Collections.Generic;
using cfEngine.Pooling;
using cfEngine.Util;

namespace cfEngine.Rt
{
    public class SubscriptionHandle
    {
        private static ListPool<IRelayBinding> bindingListPool = new();

        private List<IRelayBinding> _bindings;
        private List<IRelayBinding> bindings => _bindings ??= bindingListPool.Get();

        public void AddBinding(IRelayBinding binding)
        {
            bindings.Add(binding);
        }

        public void Unsubscribe()
        {
            foreach (var binding in _bindings)
            {
                binding.Enable(false);
            }

            bindingListPool.Release(_bindings);
        }
    }

    public static class SubscriptionHandleExtension
    {
        public static void UnsubscribeIfNotNull(this SubscriptionHandle handle)
        {
            handle?.Unsubscribe();
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
            var handle = new SubscriptionHandle();
            if (onAdd != null)
                handle.AddBinding(OnAddRelay.BindListener(onAdd));

            if (onRemove != null)
                handle.AddBinding(OnRemoveRelay.BindListener(onRemove));

            if (onUpdate != null)
                handle.AddBinding(OnUpdateRelay.BindListener(onUpdate));

            if (onDispose != null) 
                handle.AddBinding(OnDisposeRelay.BindListener(onDispose));

            return handle;
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