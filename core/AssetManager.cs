using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using cfEngine.Logging;

namespace cfEngine.Asset
{
    public abstract class AssetManager<TBaseObject>: IDisposable where TBaseObject: class
    {
        private Dictionary<string, Task> _assetLoadingTasks = new();
        private Dictionary<string, WeakReference<TBaseObject>> _assetMap = new();

        public T Load<T>(string path) where T: TBaseObject 
        {
            if (TryGetAsset(path, out var obj) && obj is T t)
            {
                return t;
            }
            
            t = _Load<T>(path);
            _assetMap[path] = new WeakReference<TBaseObject>(t);
            return t;
        }

        protected abstract T _Load<T>(string path) where T : TBaseObject;

        public async Task<T> LoadAsync<T>(string path, CancellationToken token) where T: TBaseObject
        {
            if (_assetLoadingTasks.TryGetValue(path, out var t))
            {
                if (t is not Task<T> cachedObjectTask)
                {
                    Log.LogWarning($"Detect async loading different task result type but with same path {path}");
                } 
                else if(!cachedObjectTask.IsFaulted && !cachedObjectTask.IsCanceled)
                {
                    return await cachedObjectTask;
                }
            }
            
            var objectTask = _LoadAsync<T>(path, token);
            _assetLoadingTasks[path] = objectTask;
            
            var result = await objectTask;
            
            _assetMap[path] = new WeakReference<TBaseObject>(result);
            return result;
        }

        protected abstract Task<T> _LoadAsync<T>(string path, CancellationToken token) where T : TBaseObject;
        
        public bool TryGetAsset(string path, out TBaseObject asset)
        {
            asset = null;
            return _assetMap.TryGetValue(path, out var wr) && wr.TryGetTarget(out asset);
        }

        public void Dispose()
        {
            foreach (var task in _assetLoadingTasks.Values)
            {
                task.Dispose();
            }
            
            _assetLoadingTasks.Clear();
            
            foreach (var wr in _assetMap.Values)
            {
                if (wr.TryGetTarget(out var t) && t is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            
            _assetMap.Clear();
        }
    }
}