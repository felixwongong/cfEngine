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

        public abstract string __GetDebugInfo();
    }
    
    public partial class SubscriptionBinding<TDelegate>
    {
        public override string __GetDebugInfo()
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
        public override string __GetDebugInfo()
        {
            var sb = new StringBuilder("SubscriptionGroup:");
            foreach (var subscription in _subscriptions)
            {
                sb.AppendLine(subscription.__GetDebugInfo());
            }

            return sb.ToString();
        }
    }
}

#endif