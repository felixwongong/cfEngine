using System.Collections;
using cfEngine.Pooling;

namespace cfEngine.DataStructure
{
    public class WeakReferenceList<T>: IEnumerable<T> where T : class
    {
        private static readonly ObjectPool<WeakReference<T>> _pool = new(Create, Get, Release);
        private static WeakReference<T> Create() => new WeakReference<T>(null!);
        private static void Get(WeakReference<T> @ref) => @ref.SetTarget(null!);
        private static void Release(WeakReference<T> @ref) => @ref.SetTarget(null!);

        private readonly List<WeakReference<T>> _refItems = new();
        public IReadOnlyList<WeakReference<T>> refItems => _refItems;

        public void Add(T item)
        {
            var weakRef = _pool.Get();
            weakRef.SetTarget(item);
            _refItems.Add(weakRef);
        }

        public void Flush()
        {
            int write = 0;
            for (int read = 0; read < _refItems.Count; read++)
            {
                var weakRef = _refItems[read];
                if (weakRef.TryGetTarget(out var target))
                {
                    _refItems[write] = weakRef;
                    write++;
                }
                else {
                    _pool.Release(weakRef);
                }
            }

            if (write < _refItems.Count)
                _refItems.RemoveRange(write, _refItems.Count - write);

            if(_refItems.Count < _refItems.Capacity / 2)
                _refItems.TrimExcess();
        }

        public void Remove(T item)
        {
            int write = 0;
            for (int read = 0; read < _refItems.Count; read++)
            {
                var weakRef = _refItems[read];
                if (weakRef.TryGetTarget(out var target) && !ReferenceEquals(target, item))
                {
                    _refItems[write] = weakRef;
                    write++;
                }
                else {
                    _pool.Release(weakRef);
                }
            }

            if (write < _refItems.Count)
                _refItems.RemoveRange(write, _refItems.Count - write);

            if(_refItems.Count < _refItems.Capacity / 2)
                _refItems.TrimExcess();
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var wr in _refItems)
            {
                if (wr.TryGetTarget(out var t))
                {
                    yield return t;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}