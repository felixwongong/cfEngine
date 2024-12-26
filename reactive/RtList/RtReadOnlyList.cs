using System;
using System.Collections;
using System.Collections.Generic;

namespace cfEngine.Rt
{
    public abstract partial class RtReadOnlyList<T>: RtCollection<(int index, T item)>, IReadOnlyList<T>
    {
        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public abstract int Count { get; }

        public abstract T this[int index] { get; }

        public static implicit operator RtReadOnlyList<object> (RtReadOnlyList<T> list)
        {
            return list.select(t => (object)t);
        }
    }
}