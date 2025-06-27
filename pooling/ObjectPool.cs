using System;
using System.Collections.Generic;

namespace cfEngine.Pooling
{
    public interface IObjectPool: IDisposable
    {
        
    }
    
    public class ObjectPool<T>: IObjectPool where T: class
    {
        public struct Handle : IDisposable
        {
            public static Handle Empty => new Handle(null, null);
            private readonly Action<T> _releaseAction;
            private readonly T _obj;

            public Handle(Action<T> releaseAction, T obj)
            {
                _releaseAction = releaseAction;
                _obj = obj;
            }
            public void Dispose()
            {
                _releaseAction?.Invoke(_obj);
            }
        }
        
        private readonly Func<T> _createMethod;
        private readonly Action<T> _releaseAction;

        protected readonly Queue<T> Queue = new();

        public ObjectPool(Func<T> createMethod, Action<T> releaseAction, int warmupSize = 0)
        {
            this._createMethod = createMethod;
            this._releaseAction = releaseAction;

            for (int i = 0; i < warmupSize; i++)
            {
                var instance = _createMethod();
                releaseAction(instance);
            }
        }

        public virtual T Get()
        {
            if (!Queue.TryDequeue(out var result))
            {
                return _createMethod();
            }

            return result;
        }

        public virtual Handle Get(out T value)
        {
            value = Get();
            return new Handle(_releaseAction, value);
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