using System;
using System.Collections;
using System.Collections.Generic;

namespace cfEngine.Rt
{
    public abstract class RtReadOnlyList<T>: IReadOnlyList<T>, IDisposable
    {
        private CollectionEvents<(int index, T item)> _collectionEvents;
        protected CollectionEvents<(int index, T item)> CollectionEvents => _collectionEvents ??= new CollectionEvents<(int index, T item)>();
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