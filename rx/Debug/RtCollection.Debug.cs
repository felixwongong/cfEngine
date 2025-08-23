#if CF_REACTIVE_DEBUG

using System;
using cfEngine.Util;

namespace cfEngine.Rx
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

        public string __GetDebugTitle()
        {
            return GetType().GetTypeName();
        }

        public void __SetCollectionId(Guid collectionId)
        {
            _collectionId = collectionId;
        }
    }

    public interface ICollectionDebug : IMarkedDebug
    {
        public Guid __GetSourceId();
        public bool __IsRoot() => __GetSourceId() == Guid.Empty;
        public void Dispose();
    }
    
    public abstract partial class RxCollection<TEventArgs>: ICollectionDebug
    {
        private Guid __sourceId = Guid.Empty;
            
        private Guid __id = Guid.Empty;
        
        public string __name { get; private set; }
        
        public Guid __GetId()
        {
            if (__id.Equals(Guid.Empty))
            {
                __id = Guid.NewGuid();
                CollectionEvents.__SetCollectionId(__id);
            }

            return __id;
        }

        public Guid __GetSourceId() => __sourceId;

        public string __GetDebugTitle()
        {
            if (string.IsNullOrEmpty(__name))
            {
                return GetType().GetTypeName();
            } 
            
            return __name;
        }
        
        public void __SetSourceCollectionId<TSource>(TSource source) where TSource: IMarkedDebug
        {
            __sourceId = source.__GetId();
            _RxDebug.Instance.RecordMutatedReference(source.__GetId(), __id);
        }
        
        public void __SetDebugName(string name)
        {
            this.__name = name;
        }
    }
}

#endif