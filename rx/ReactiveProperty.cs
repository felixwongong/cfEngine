using System;

namespace cfEngine.Rx
{
    /// <summary>
    /// A simple reactive container that holds a single value and dispatches
    /// to subscribers when the value changes. Built on <see cref="Relay{T}"/>
    /// from the existing cfEngine.Rx event system.
    /// </summary>
    public class ReactiveProperty<T>
    {
        private T _value;
        private Relay<T> _changedRelay;

        public ReactiveProperty(T initialValue = default) => _value = initialValue;

        public T Value
        {
            get => _value;
            set
            {
                if (EqualityComparer<T>.Default.Equals(_value, value))
                    return;
                _value = value;
                _changedRelay?.Dispatch(_value);
            }
        }

        public Subscription Subscribe(Action<T> handler)
        {
            _changedRelay ??= new Relay<T>(this);
            var sub = _changedRelay.AddListener(handler);
            handler(_value);
            return sub;
        }

        public object GetValue() => _value;
    }
}
