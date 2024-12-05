using System;
using System.Collections.Generic;
using cfEngine.Util;

namespace cfEngine.Rt
{
    public partial class CollectionEventsBase
    {
        public class Debug
        {
            private static Debug _instance;
            public static Debug Instance => _instance ??= new Debug();
            
            private Dictionary<CollectionEventsBase, Dictionary<int, WeakReference<Subscription>>> _recordedEvents = new();
            public IReadOnlyDictionary<CollectionEventsBase, Dictionary<int, WeakReference<Subscription>>> recorded => _recordedEvents;

            public void Record(CollectionEventsBase events, Subscription subscription)
            {
                if (!_recordedEvents.TryGetValue(events, out var subscriptions))
                {
                    subscriptions = new Dictionary<int, WeakReference<Subscription>>();
                    _recordedEvents.Add(events, subscriptions);
                }

                subscriptions.Add(subscription.Id, new WeakReference<Subscription>(subscription));
            }   
        }
    }
}