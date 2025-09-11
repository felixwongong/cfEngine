using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

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
        
        private readonly Func<T> _create;
        private readonly Action<T> _init;
        private readonly Action<T> _release;
        private readonly Action<T> _destroy;

        protected readonly Queue<T> Queue = new();

        public ObjectPool(Func<T> create, Action<T> init, Action<T> release, Action<T> destroy, int warmupSize = 0)
        {
            _create = create;
            _init = init;
            _release = release;
            _destroy = destroy;

            for (int i = 0; i < warmupSize; i++)
            {
                var instance = Create();
                release(instance);
            }
        }

        public virtual T Get()
        {
            if (!Queue.TryDequeue(out var result))
            {
                return Create();
            }

            _init?.Invoke(result);
            return result;
        }

        public virtual Handle Get(out T value)
        {
            value = Get();
            return new Handle(_release, value);
        }

        public virtual void Release(T obj)
        {
            _release(obj);
            Queue.Enqueue(obj);
        }
        
        public virtual void FlushToRemain(int remain)
        {
            while (Queue.Count > remain)
            {
                var obj = Queue.Dequeue();
                _destroy?.Invoke(obj);
            }
        }

        public virtual void Dispose()
        {
            FlushToRemain(0);
        }

        private T Create()
        {
            var inst = _create();
            _init?.Invoke(inst);
            return inst;
        }
    }
}