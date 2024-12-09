using System;
using cfEngine.Logging;

namespace cfEngine.Rt
{
    public abstract partial class RtCollection<TEventArgs>: IDisposable
    {
        private CollectionEvents<TEventArgs> _collectionEvents;
        protected CollectionEvents<TEventArgs> CollectionEvents => _collectionEvents ??= new CollectionEvents<TEventArgs>();
        public ICollectionEvents<TEventArgs> Events => CollectionEvents;

        protected RtCollection()
        {
#if CF_REACTIVE_DEBUG
            if (UnityEditor.EditorApplication.isPlaying)
            {
                _RtDebug.Instance.RecordCollection(this);
            }
#endif
        }

        public virtual void Dispose()
        {
            if (_collectionEvents != null)
            {
                _collectionEvents.OnDisposeRelay.Dispatch();
                _collectionEvents.Dispose();
                _collectionEvents = null;
            }
        }

        ~RtCollection()
        {
            if (_collectionEvents != null && (_collectionEvents.OnAddRelay.listenerCount > 0 ||
                                              _collectionEvents.OnRemoveRelay.listenerCount > 0 ||
                                              _collectionEvents.OnUpdateRelay.listenerCount > 0))
            {
                Log.LogError($"{this}.Finalizer, it was not disposed properly!");
                Dispose();
            }
        }
    }
}