using System;
using System.Collections.Generic;
using cfEngine.Logging;

namespace cfEngine.Util
{
    public abstract class Subscription
    {
        public abstract void Unsubscribe();
    }

    public class SubscriptionGroup : Subscription
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
    
    public class SubscriptionBinding<TDelegate>: Subscription where TDelegate: class
    {
        public readonly WeakReference<TDelegate> ListenerRef;
        private readonly RelayBase<TDelegate> _relay;
        public SubscriptionBinding(TDelegate listener, RelayBase<TDelegate> relay)
        {
            ListenerRef = new WeakReference<TDelegate>(listener);
            _relay = relay;
        }

        public bool HasListener(TDelegate d)
        {
            return HasListener(out var listener) && listener.Equals(d);
        }
        
        public bool HasListener(out TDelegate listener)
        {
            return ListenerRef.TryGetTarget(out listener);
        }
        
        public override void Unsubscribe()
        {
            if(ListenerRef.TryGetTarget(out var listener))
                _relay.RemoveListener(listener);
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

        public RelayBase(int defaultSize = 1)
        {
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
                _cap *= 2;
                _count = Expand(ref _subscriptionRefList);
            }

            var subscription = new SubscriptionBinding<TDelegate>(listener, this);
            var subscriptionRef = new WeakReference<SubscriptionBinding<TDelegate>>(subscription);
            _subscriptionRefList[_count] = subscriptionRef;

            return subscription;
        }

        public bool RemoveListener(TDelegate listener)
        {
            bool result = false;
            for (var i = 0; i < _cap; i++)
            {
                if (_subscriptionRefList[i] != null &&
                    _subscriptionRefList[i].TryGetTarget(out var subscription) &&
                    subscription.HasListener(listener))
                {
                    _subscriptionRefList[i] = null;
                    _count--;
                    result = true;
                }
            }

            return result;
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
                if (bindingRef != null && bindingRef.TryGetTarget(out var binding) && binding.HasListener(target))
                {
                    return true;
                }
            }

            return false;
        }

        public int Expand(ref WeakReference<SubscriptionBinding<TDelegate>>[] bindings)
        {
            var newCap = _cap * 2;
            
            var newBindings = new WeakReference<SubscriptionBinding<TDelegate>>[newCap];
            var newCount = 0;
            for (var i = 0; i < bindings.Length; i++)
            {
                if (bindings[i] != null && bindings[i].TryGetTarget(out var binding) && binding.HasListener(out _))
                {
                   newBindings[newCount++] = bindings[i]; 
                }
            }

            return newCount;
        }
    }
    
    public class Relay: RelayBase<Action>
    {
        public void Dispatch()
        {
            int newCount = 0;
            for (int i = 0; i < _cap;)
            {
                if (_subscriptionRefList[i] == null || !_subscriptionRefList[i].TryGetTarget(out var subscription) || !subscription.HasListener(out var listener))
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
                    listener?.Invoke();
                    newCount++;
                }

                i++;
            }

            _count = newCount;
        }
    }
    
    public class Relay<T>: RelayBase<Action<T>>
    {
        public void Dispatch(T value1)
        {
            int newCount = 0;
            for (int i = 0; i < _cap;)
            {
                if (_subscriptionRefList[i] == null || !_subscriptionRefList[i].TryGetTarget(out var subscription) || !subscription.HasListener(out var listener))
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
                    listener?.Invoke(value1);
                    newCount++;
                }

                i++;
            }

            _count = newCount;
        }
    }

    public class Relay<T1, T2> : RelayBase<Action<T1, T2>>
    {
        public void Dispatch(T1 value1, T2 value2)
        { 
            int newCount = 0;
            for (int i = 0; i < _cap;)
            {
                if (_subscriptionRefList[i] == null || !_subscriptionRefList[i].TryGetTarget(out var subscription) || !subscription.HasListener(out var listener))
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
                    listener?.Invoke(value1, value2);
                    newCount++;
                }

                i++;
            }

            _count = newCount;
        }
    }
}