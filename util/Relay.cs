using System;
using System.Collections.Generic;
using cfEngine.Logging;

namespace cfEngine.Rt
{
    public abstract partial class Subscription
    {
        public abstract void Unsubscribe();
    }

    public partial class SubscriptionGroup : Subscription
    {
        private List<Subscription> _subscriptions = new List<Subscription>();
        
        public void Add(Subscription subscription)
        {
            _subscriptions.Add(subscription);
        }
        
        public override void Unsubscribe()
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.UnsubscribeIfNotNull();
            }
            
            _subscriptions.Clear();
        }
    }
    
    public partial class SubscriptionBinding<TDelegate>: Subscription where TDelegate: class
    {
        public readonly TDelegate Listener;
        private readonly RelayBase<TDelegate> _relay;
        public SubscriptionBinding(TDelegate listener, RelayBase<TDelegate> relay)
        {
            Listener = listener;
            _relay = relay;
        }

        public bool IsListener(TDelegate d)
        {
            return Listener.Equals(d);
        }
        
        public override void Unsubscribe()
        {
            _relay?.RemoveListener(Listener);
        }
    }

    public static class SubscriptionHandleExtension
    {
        public static void UnsubscribeIfNotNull(this Subscription binding)
        {
            binding?.Unsubscribe();
        }
    }
    
    public abstract class RelayBase<TDelegate> where TDelegate : class
    {
        protected WeakReference<SubscriptionBinding<TDelegate>>[] _subscriptionRefList;
        protected int _cap;
        protected int _count;

        public int listenerCount => _count;
        
#pragma warning disable 0414
        private readonly object _o;
#pragma warning restore 0414

        public RelayBase(object owner, int defaultSize = 1)
        {
            _o = owner;
            _cap = defaultSize;
            _subscriptionRefList = new WeakReference<SubscriptionBinding<TDelegate>>[_cap];
        }

        public Subscription AddListener(TDelegate listener)
        {
            if (Contains(listener))
            {
                Log.LogError("SmartRelayBase.AddListener: Listener already exists");
                return null;
            }

            if (_count == _cap)
            {
                _count = Expand(ref _subscriptionRefList);
            }

            var subscription = new SubscriptionBinding<TDelegate>(listener, this);
            var subscriptionRef = new WeakReference<SubscriptionBinding<TDelegate>>(subscription);
            _subscriptionRefList[_count++] = subscriptionRef;

            return subscription;
        }

        public bool RemoveListener(TDelegate listener)
        {
            bool result = false;
            for (var i = 0; i < _cap; i++)
            {
                if (_subscriptionRefList[i] != null &&
                    _subscriptionRefList[i].TryGetTarget(out var subscription) &&
                    subscription.IsListener(listener))
                {
                    _subscriptionRefList[i] = null;
                    _count--;
                    result = true;
                }
            }

            return result;
        }

        public void RemoveAll()
        {
            for (var i = 0; i < _subscriptionRefList.Length; i++)
            {
                _subscriptionRefList[i] = null;
            }

            _cap = _subscriptionRefList.Length;
        }

        public bool Contains(TDelegate d)
        {
            if (_cap > _subscriptionRefList.Length)
            {
                Log.LogException(new IndexOutOfRangeException("SmartRelayBase.Contains: Bound exceeded the length of the subscription array"));
                return false;
            }

            return Contains(_subscriptionRefList.AsSpan(0, _cap), d);
        }

        private bool Contains(Span<WeakReference<SubscriptionBinding<TDelegate>>> bindings, TDelegate target)
        {
            foreach (var bindingRef in bindings)
            {
                if (bindingRef != null && bindingRef.TryGetTarget(out var binding) && binding.IsListener(target))
                {
                    return true;
                }
            }

            return false;
        }

        public int Expand(ref WeakReference<SubscriptionBinding<TDelegate>>[] bindings)
        {
            _cap *= 2;
            
            var newBindings = new WeakReference<SubscriptionBinding<TDelegate>>[_cap];
            var newCount = 0;
            for (var i = 0; i < bindings.Length; i++)
            {
                if (bindings[i] != null && bindings[i].TryGetTarget(out _))
                {
                   newBindings[newCount++] = bindings[i]; 
                }
            }
            
            bindings = newBindings;

            return newCount;
        }
    }
    
    public class Relay: RelayBase<Action>
    {
        public Relay(object owner, int defaultSize = 1) : base(owner, defaultSize)
        {
        }
        public void Dispatch()
        {
            int newCount = 0;
            for (int i = 0; i < _cap;)
            {
                if (_subscriptionRefList[i] == null || !_subscriptionRefList[i].TryGetTarget(out var subscription))
                {
                    _subscriptionRefList[i] = null;
                    if (i + 1 < _cap)
                    {
                        _subscriptionRefList[i] = _subscriptionRefList[i + 1];
                        continue;
                    }
                }
                else
                {
                    subscription.Listener?.Invoke();
                    newCount++;
                }

                i++;
            }

            _count = newCount;
        }
    }
    
    public class Relay<T>: RelayBase<Action<T>>
    {
        public Relay(object owner, int defaultSize = 1) : base(owner, defaultSize)
        {
        }
        public void Dispatch(T value1)
        {
            int newCount = 0;
            for (int i = 0; i < _cap;)
            {
                if (_subscriptionRefList[i] == null || !_subscriptionRefList[i].TryGetTarget(out var subscription))
                {
                    _subscriptionRefList[i] = null;
                    if (i + 1 < _cap)
                    {
                        _subscriptionRefList[i] = _subscriptionRefList[i + 1];
                        continue;
                    }
                }
                else
                {
                    subscription.Listener?.Invoke(value1);
                    newCount++;
                }

                i++;
            }

            _count = newCount;
        }
    }

    public class Relay<T1, T2> : RelayBase<Action<T1, T2>>
    {
        public Relay(object owner, int defaultSize = 1) : base(owner, defaultSize)
        {
        }
        public void Dispatch(T1 value1, T2 value2)
        { 
            int newCount = 0;
            for (int i = 0; i < _cap;)
            {
                if (_subscriptionRefList[i] == null || !_subscriptionRefList[i].TryGetTarget(out var subscription))
                {
                    _subscriptionRefList[i] = null;
                    if (i + 1 < _cap)
                    {
                        _subscriptionRefList[i] = _subscriptionRefList[i + 1];
                        continue;
                    }
                }
                else
                {
                    subscription.Listener?.Invoke(value1, value2);
                    newCount++;
                }

                i++;
            }

            _count = newCount;
        }
    }
}