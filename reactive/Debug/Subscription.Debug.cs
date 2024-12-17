#if CF_REACTIVE_DEBUG

using System;
using System.Text;
using cfEngine.Util;

namespace cfEngine.Rt
{
    public abstract partial class Subscription: IMarkedDebug
    {
        private Guid __Id = Guid.Empty;
        public Guid __GetId()
        {
            if (__Id.Equals(Guid.Empty))
            {
                __Id = Guid.NewGuid();
            }
            
            return __Id;
        }

        public abstract string __GetDebugTitle();
    }
    
    public partial class SubscriptionBinding<TDelegate>
    {
        public override string __GetDebugTitle()
        {
            if (ListenerRef.TryGetTarget(out var listener))
            {
                return listener is Delegate d ? $"{d.Method.Name}: {d.Target.GetType().GetTypeName()}" : listener.GetType().GetTypeName();
            }
            else
            {
                return "No listener in subscription";
            }
        }
    }

    public partial class SubscriptionGroup
    {
        public override string __GetDebugTitle()
        {
            var sb = new StringBuilder("SubscriptionGroup:");
            foreach (var subscription in _subscriptions)
            {
                sb.AppendLine(subscription.__GetDebugTitle());
            }

            return sb.ToString();
        }
    }
}

#endif