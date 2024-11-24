using System;
using System.Collections;
using System.Collections.Generic;

namespace cfEngine.Rt
{
    public abstract class RtReadOnlyList<T>: IReadOnlyList<T>, IDisposable
    {
        protected readonly CollectionEvents<(int index, T item)> CollectionEvents = new();
        public ICollectionEvents<(int index, T item)> Events => CollectionEvents;

        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public abstract int Count { get; }

        public abstract T this[int index] { get; }

        public virtual void Dispose()
        {
            CollectionEvents.OnDisposeRelay.Dispatch();
            CollectionEvents?.Dispose();
        }
        
        public static implicit operator RtReadOnlyList<object> (RtReadOnlyList<T> list)
        {
            return list.SelectLocal(t => (object)t);
        }
    }
}