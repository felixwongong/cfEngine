using System;
using cfEngine.Util;

namespace cfEngine.Rt
{
    public static class CollectionEventExtension
    {
        public static Subscription OnChange<T>(this ICollectionEvents<T> collectionEvents, Action action)
        {
            var handle = collectionEvents.Subscribe(
                _ => action(),
                _ => action(),
                (_, _) => action(),
                null
            );
            return handle;
        }

        public static Subscription Subscribe<T>(this ICollectionEvents<T> collectionEvents, Action<T> onAdd = null, Action<T> onRemove = null, Action<T, T> onUpdate = null, Action onDispose = null)
        {
            var handle = new SubscriptionGroup();
            if (onAdd != null)
                handle.Add(collectionEvents.SubscribeOnAdd(onAdd));

            if (onRemove != null)
                handle.Add(collectionEvents.SubscribeOnRemove(onRemove));

            if (onUpdate != null)
                handle.Add(collectionEvents.SubscribeOnUpdate(onUpdate));

            if (onDispose != null) 
                handle.Add(collectionEvents.SubscribeOnDispose(onDispose));

            return handle;
        }
    }
}