using System;
using System.Collections;
using System.Collections.Generic;

namespace cfEngine.Rx
{
    public abstract partial class RxReadOnlyList<T>: RxCollection<(int index, T item)>, IReadOnlyList<T>
    {
        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public abstract int Count { get; }

        public abstract T this[int index] { get; }

        public static implicit operator RxReadOnlyList<object> (RxReadOnlyList<T> list)
        {
            return list.select(t => (object)t);
        }
    }
}