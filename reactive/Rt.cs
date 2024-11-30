using System.Collections.Generic;

namespace cfEngine.Rt
{
    public class Rt<T>: RtReadOnlyList<T>
    {
        public T Value { get; private set; }

        public Rt()
        {
        }
        
        public Rt(T defaultValue)
        {
            Value = defaultValue;
        }
        
        public void Set(T value)
        {
            var oldValue = Value;
            Value = value;
            CollectionEvents.OnUpdateRelay.Dispatch((0, oldValue), (0, value));
        }

        public void SetNoTrigger(T value)
        {
            Value = value;
        }

        public static implicit operator T(Rt<T> rt) => rt.Value;
        public override IEnumerator<T> GetEnumerator()
        {
            yield return Value;
        }

        public override int Count => 1;

        public override T this[int index] => Value;
    }
}