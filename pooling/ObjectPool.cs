using System;
using System.Collections.Generic;

namespace cfEngine.Pooling
{
    public interface IObjectPool: IDisposable
    {
        
    }
    
    public class ObjectPool<T>: IObjectPool where T: class
    {
        private readonly Func<T> _createMethod;
        private readonly Action<T> _releaseAction;

        protected readonly Queue<T> Queue = new();

        public ObjectPool(Func<T> createMethod, Action<T> releaseAction)
        {
            this._createMethod = createMethod;
            this._releaseAction = releaseAction;
        }

        public virtual T Get()
        {
            if (!Queue.TryDequeue(out var result))
            {
                return _createMethod();
            }

            return result;
        }

        public virtual void Release(T obj)
        {
            _releaseAction(obj);
            Queue.Enqueue(obj);
        }

        public virtual void Dispose()
        {
            Queue.Clear();
        }
    }
}