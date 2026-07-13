using System;

namespace cfEngine.Rx
{
    public abstract partial class RxCollection<TEventArgs>: IDisposable
    {
        private CollectionEvents<TEventArgs>? _collectionEvents;
        protected CollectionEvents<TEventArgs> CollectionEvents => _collectionEvents ??= new CollectionEvents<TEventArgs>(this);
        public ICollectionEvents<TEventArgs> Events => CollectionEvents;

        protected RxCollection()
        {
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
    }
}