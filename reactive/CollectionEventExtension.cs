using System;

namespace cfEngine.Rt
{
    public static class CollectionEventExtension
    {
        public static SubscriptionHandle OnChange<T>(this ICollectionEvents<T> collectionEvents, Action action)
        {
            return collectionEvents.Subscribe(
                _ => action(),
                _ => action(),
                (_, _) => action()
            );
        }
    }
}