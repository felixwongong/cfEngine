using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using cfEngine.Logging;

namespace cfEngine.Pooling
{
    public class PoolManager: IDisposable
    {
        private readonly Dictionary<string, IObjectPool> _poolMap = new();

        public bool TryGetPool(string key, out IObjectPool pool)
        {
            return _poolMap.TryGetValue(key, out pool);
        }

        public bool TryGetPool<T>(string key, out T pool) where T : class, IObjectPool 
        {
            pool = null;
            if (TryGetPool(key, out var x))
            {
                if (x is T t)
                {
                    pool = t;
                    return true;
                }
                else
                {
                    Log.LogException(new ArgumentNullException(nameof(key), $"try get pool key with different pool type, original: {x.GetType()}, new: {typeof(T)}"));
                    return false;
                }
            }

            return false;
        }

        public void AddPool<T>(string key, T pool) where T : IObjectPool
        {
            if (TryGetPool(key, out _))
            {
                throw new ArgumentException($"Pool already exist, key: {key}");
            }

            _poolMap[key] = pool;
        }

        public T GetOrCreatPool<T>(string key, [NotNull] Func<T> createFunc) where T: class, IObjectPool
        {
            if (TryGetPool<T>(key, out var pool)) return pool;

            pool = createFunc();
            AddPool(key, pool);
            return pool;
        }

        public void Dispose()
        {
            foreach (var pool in _poolMap.Values)
            {
                pool.Dispose();
            }
            
            _poolMap.Clear();
        }
    }
}