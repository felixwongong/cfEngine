using System;
using System.Diagnostics;
using cfEngine.Logging;
using cfEngine.Util;

namespace cfEngine.Rx
{
    public abstract partial class RtCollection<TEventArgs>: IDisposable
    {
        private CollectionEvents<TEventArgs> _collectionEvents;
        protected CollectionEvents<TEventArgs> CollectionEvents => _collectionEvents ??= new CollectionEvents<TEventArgs>(this);
        public ICollectionEvents<TEventArgs> Events => CollectionEvents;

        protected RtCollection()
        {
#if CF_REACTIVE_DEBUG
            _RtDebug.Instance.RecordCollection(this);
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

#if CF_REACTIVE_DEBUG
            _RtDebug.Instance.RemoveCollectionRecord(this);
#endif
        }
    }
}