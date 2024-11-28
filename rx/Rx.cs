using System;

namespace cfEngine.Rt
{
    public class Rx<T>: IDisposable
    {
        public T Value { get; private set; }

        private CollectionEvents<T> CollectionEvents = new();
        public ICollectionEvents<T> Events => CollectionEvents;

        public Rx()
        {
        }
        
        public Rx(T defaultValue)
        {
            Value = defaultValue;
        }
        
        public void Set(T value)
        {
            var oldValue = Value;
            Value = value;
            CollectionEvents.OnUpdateRelay.Dispatch(oldValue, Value);
        }

        public void SetNoTrigger(T value)
        {
            Value = value;
        }

        public void Dispose()
        {
            Value = default;
            CollectionEvents.Dispose();
            CollectionEvents = null;
        }
        
        public static implicit operator T(Rx<T> rx) => rx.Value;
    }
}