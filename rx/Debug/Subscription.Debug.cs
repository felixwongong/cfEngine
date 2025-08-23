#if CF_REACTIVE_DEBUG

using System;
using System.Text;
using cfEngine.Util;

namespace cfEngine.Rx
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
            return Listener is Delegate d ? $"{d.Target.GetType().GetTypeName()}.{d.Method.Name}" : Listener.GetType().GetTypeName();
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