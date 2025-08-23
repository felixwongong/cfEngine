namespace cfEngine.Rx
{
    public abstract partial class RxCollection<TEventArgs>: IDisposable
    {
        private CollectionEvents<TEventArgs>? _collectionEvents;
        protected CollectionEvents<TEventArgs> CollectionEvents => _collectionEvents ??= new CollectionEvents<TEventArgs>(this);
        public ICollectionEvents<TEventArgs> Events => CollectionEvents;

        protected RxCollection()
        {
#if CF_REACTIVE_DEBUG
            _RxDebug.Instance.RecordCollection(this);
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
            _RxDebug.Instance.RemoveCollectionRecord(this);
#endif
        }
    }
}