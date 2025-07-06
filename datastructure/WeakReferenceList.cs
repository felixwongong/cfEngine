using System;
using System.Collections.Generic;
using cfEngine.Pooling;

namespace cfEngine.DataStructure
{
    public class WeakReferenceListPool<T> : ObjectPool<WeakReferenceList<T>> where T : class
    {
        private static WeakReferenceList<T> createMethod()
        {
            return new WeakReferenceList<T>();
        }
        
        private static void releaseAction(WeakReferenceList<T> list)
        {
            list.Clear();
        }
        
        public WeakReferenceListPool() : base(createMethod, null, releaseAction)
        {
        }
    }
    
    public class WeakReferenceList<T> : List<WeakReference<T>> where T : class
    {
        private static WeakReferenceListPool<T> _pool;
        private static WeakReferenceListPool<T> GetPool => _pool ??= new WeakReferenceListPool<T>();
        
        public static WeakReferenceList<T> Create()
        {
            return GetPool.Get();
        }
        
        public void Add(T item)
        {
            Add(new WeakReference<T>(item));
        }

        public void Remove(T item)
        {
            RemoveAll(weakRef =>
            {
                if (weakRef.TryGetTarget(out var target))
                {
                    return target == item;
                }

                return true;
            });

            TrimExcess();
            if (Count == 0)
            {
                GetPool.Release(this);
            }
        }
    }
}