using System;
using System.Collections;
using System.Collections.Generic;
using cfEngine.Logging;

namespace cfEngine.Rt
{
    public abstract partial class RtReadOnlyList<T>: IReadOnlyList<T>, IDisposable
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
            _collectionEvents?.OnDisposeRelay.Dispatch();
            _collectionEvents = null;
        }
        
        public static implicit operator RtReadOnlyList<object> (RtReadOnlyList<T> list)
        {
            return list.select(t => (object)t);
        }

        protected RtReadOnlyList()
        {
#if UNITY_EDITOR
            _RtDebug.Instance.RecordCollection(this);
#endif 
        }

        ~RtReadOnlyList()
        {
            if (_collectionEvents != null && 
                (_collectionEvents.OnAddRelay.listenerCount > 0 || 
                _collectionEvents.OnRemoveRelay.listenerCount > 0 ||
                _collectionEvents.OnUpdateRelay.listenerCount > 0))
            {
                Log.LogError($"{this}.Finalizer, it was not disposed properly!");
                Dispose();
            }
        }
    }
}