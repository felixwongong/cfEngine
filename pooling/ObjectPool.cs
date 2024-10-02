using System;
using System.Collections.Generic;

namespace cfEngine.Pooling
{
    public class ObjectPool<T> where T: class
    {
        private readonly Func<T> _createMethod;
        private readonly Action<T> _releaseAction;

        private Queue<T> _queue = new();

        public ObjectPool(Func<T> createMethod, Action<T> releaseAction)
        {
            this._createMethod = createMethod;
            this._releaseAction = releaseAction;
        }

        public T Get()
        {
            if (!_queue.TryDequeue(out var result))
            {
                return _createMethod();
            }

            return result;
        }

        public void Release(T obj)
        {
            _releaseAction(obj);
            _queue.Enqueue(obj);
        }
    }
}