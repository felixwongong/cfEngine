using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using cfEngine.Logging;

namespace cfEngine.Asset
{
    public abstract class AssetManager
    {
        private Dictionary<string, Task> _assetLoadingTasks = new();
        private Dictionary<string, WeakReference<object>> _assetMap = new();

        public object Load(string path)
        {
            if (TryGetAsset(path, out var obj))
            {
                return obj;
            }
            
            obj = _Load(path);
            _assetMap[path] = new WeakReference<object>(obj);
            return obj;
        }

        protected abstract object _Load(string path);
        
        public T Load<T>(string path) where T : class
        {
            if (TryGetAsset(path, out var obj) && obj is T t)
            {
                return t;
            }
            
            t = _Load<T>(path);
            _assetMap[path] = new WeakReference<object>(t);
            return t;
        }

        protected abstract T _Load<T>(string path) where T : class;

        public Task<object> LoadAsync(string path, CancellationToken token)
        {
            if (_assetLoadingTasks.TryGetValue(path, out var t))
            {
                if (t is not Task<object> cachedObjectTask)
                {
                    Log.LogWarning($"Detect async loading different task result type but with same path {path}");
                } 
                else if(!cachedObjectTask.IsFaulted && !cachedObjectTask.IsCanceled)
                {
                    return cachedObjectTask;
                }
            }

            var objectTask = _LoadAsync(path, token).ContinueWith(t =>
                {
                    var result = t.Result;
                    _assetMap[path] = new WeakReference<object>(result);
                    return result;
                }, token);
            _assetLoadingTasks[path] = objectTask;
            return objectTask;
        }

        protected abstract Task<object> _LoadAsync(string path, CancellationToken token);
        
        public Task<T> LoadAsync<T>(string path, CancellationToken token)
        {
            if (_assetLoadingTasks.TryGetValue(path, out var t))
            {
                if (t is not Task<T> cachedObjectTask)
                {
                    Log.LogWarning($"Detect async loading different task result type but with same path {path}");
                } 
                else if(!cachedObjectTask.IsFaulted && !cachedObjectTask.IsCanceled)
                {
                    return cachedObjectTask;
                }
            }

            var objectTask = _LoadAsync<T>(path, token).ContinueWith(t =>
                {
                    var result = t.Result;
                    _assetMap[path] = new WeakReference<object>(result);
                    return result;
                }, token);
            _assetLoadingTasks[path] = objectTask;
            return objectTask;
        }

        protected abstract Task<T> _LoadAsync<T>(string path, CancellationToken token) where T : class;
        
        public bool TryGetAsset(string path, out object asset)
        {
            asset = null;
            return _assetMap.TryGetValue(path, out var wr) && wr.TryGetTarget(out asset);
        }
    }
}