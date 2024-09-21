using System;
using System.Collections.Generic;

namespace cfEngine.Pooling
{
    public class ObjectPool<T> where T: class, new()
    {
        private readonly Func<T> _createMethod;
        private readonly Action<T> _disposeAction;

        private Queue<T> _queue = new();

        protected ObjectPool(Func<T> createMethod, Action<T> disposeAction)
        {
            this._createMethod = createMethod;
            this._disposeAction = disposeAction;
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
            _disposeAction(obj);
            _queue.Enqueue(obj);
        }
    }
}