using System;
using System.Collections;
using System.Collections.Generic;

namespace cfEngine.Rx
{
    public abstract class RtReadOnlyList<T>: IReadOnlyList<T>, IDisposable
    {
        protected readonly CollectionEvents<T> CollectionEvents = new();
        public ICollectionEvents<T> Events => CollectionEvents;

        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public abstract int Count { get; }

        public abstract T this[int index] { get; }

        public void Dispose()
        {
            CollectionEvents?.Dispose();
        }
    }
}