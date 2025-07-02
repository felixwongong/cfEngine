using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using cfEngine.Logging;
using cfEngine.Pooling;
using cfEngine.Service;

namespace cfEngine.Core
{
    public static partial class ServiceName
    {
        public const string Pool = "Pool";
    }
    
    public static partial class GameExtension
    {
        public static Game WithPoolManager(this Game game, PoolManager service)
        {
            game.Register(service, ServiceName.Pool);
            return game;
        }
        
        public static PoolManager GetPoolManager(this Game game) => game.GetService<PoolManager>(ServiceName.Pool);

        public static Res<T, Exception> GetPool<T>(this Game game, string poolKey) where T: IObjectPool
        {
            var poolManager = game.GetPoolManager();
            if (poolManager == null)
                return Res.Err<T>(new Exception("PoolManager is not registered in the game. Please call WithPoolManager() to register it."));
            
            if(!poolManager.TryGetPool(poolKey, out var pool))
                return Res.Err<T>(new KeyNotFoundException($"Pool with key '{poolKey}' not found."));

            if (pool is not T t)
                return Res.Err<T>(new InvalidCastException($"Pool with key '{poolKey}' is not of type {typeof(T)}. Found: {pool.GetType()}"));
            
            return Res.Ok(t);
        }
    }
}

namespace cfEngine.Pooling
{
    public class PoolManager: IService 
    {
        private readonly Dictionary<string, IObjectPool> _poolMap = new();

        public bool TryGetPool(string key, out IObjectPool pool)
        {
            pool = null;
            return !string.IsNullOrEmpty(key) && _poolMap.TryGetValue(key, out pool);
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

        public T GetOrCreatePool<T>(string key, [NotNull] Func<T> createFunc) where T: class, IObjectPool
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