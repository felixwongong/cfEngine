using System;
using System.Collections.Generic;
using cfEngine.Pooling;

namespace cfEngine.DataStructure
{
    public class WeakReferenceList<T> : List<WeakReference<T>> where T : class
    {
        private static ObjectPool<WeakReference<T>> _pool = new ObjectPool<WeakReference<T>>(Create, Get, Release);

        private static WeakReference<T> Create() => new WeakReference<T>(null);
        private static void Get(WeakReference<T> @ref) => @ref.SetTarget(null);
        private static void Release(WeakReference<T> @ref) => @ref.SetTarget(null);

        public void Add(T item)
        {
            var weakRef = _pool.Get();
            weakRef.SetTarget(item);
            Add(weakRef);
        }

        public void Remove(T item)
        {
            int write = 0;
            for (int read = 0; read < Count; read++)
            {
                if (this[read].TryGetTarget(out var target) && !ReferenceEquals(target, item))
                {
                    this[write] = this[read];
                    write++;
                }
            }

            if (write < Count)
            {
                for (int i = write; i < Count; i++)
                {
                    _pool.Release(this[i]);
                }
                
                RemoveRange(write, Count - write);
            }

            TrimExcess();
        }
    }
}