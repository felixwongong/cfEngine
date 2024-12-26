#if CF_REACTIVE_DEBUG

using System;
using cfEngine.Util;

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

        public string __GetDebugTitle()
        {
            return GetType().GetTypeName();
        }

        public void __SetCollectionId(Guid collectionId)
        {
            _collectionId = collectionId;
        }
    }

    public abstract partial class RtCollection<TEventArgs>: ICollectionDebug
    {
        private Guid __sourceId = Guid.Empty;
            
        private Guid __id = Guid.Empty;
        
        public string name { get; private set; }
        
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
            if (string.IsNullOrEmpty(name))
            {
                return GetType().GetTypeName();
            } 
            
            return name;
        }
        
        public void __SetSourceCollectionId<TSource>(TSource source) where TSource: IMarkedDebug
        {
            __sourceId = source.__GetId();
            _RtDebug.Instance.RecordMutatedReference(source.__GetId(), __id);
        }
        
        public void __SetDebugName(string name)
        {
            this.name = name;
        }
    }
}

#endif