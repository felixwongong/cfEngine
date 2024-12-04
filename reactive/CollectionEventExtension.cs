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
                (_, _) => action()
            );
            return handle;
        }
    }
}