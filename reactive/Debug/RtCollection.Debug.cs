#if CF_REACTIVE_DEBUG

using System;

namespace cfEngine.Rt
{
    public partial interface ICollectionEvents<out T>: IMarkedDebug
    {
    }
    
    public partial class CollectionEvents<T>: ICollectionEvents<T>
    {
        private Guid _collectionId;

        public Guid __GetId()
        {
            return _collectionId;
        }

        public string __GetDebugInfo()
        {
            return string.Empty;
        }

        public void __SetCollectionId(Guid collectionId)
        {
            _collectionId = collectionId;
        }
    }

    public abstract partial class RtCollection<TEventArgs>: IMarkedDebug
    {
        private Guid __sourceId = Guid.Empty;
            
        private Guid __id = Guid.Empty;
        
        public Guid __GetId()
        {
            if (__id.Equals(Guid.Empty))
            {
                __id = Guid.NewGuid();
                CollectionEvents.__SetCollectionId(__id);
            }

            return __id;
        }

        public string __GetDebugInfo()
        {
            return string.Empty;
        }
        
        public void __SetSourceCollectionId<TSource>(TSource source) where TSource: IMarkedDebug
        {
            __sourceId = source.__GetId();
        }
    }
}

#endif