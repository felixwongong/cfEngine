using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using cfEngine.Logging;

namespace cfEngine.Asset
{
    public abstract class AssetHandle
    {
        
    }
    public class AssetHandle<T>: AssetHandle where T : class
    {
        public readonly WeakReference<T> Asset;
        public readonly Action ReleaseAction;

        public AssetHandle(T asset, Action releaseAction)
        {
            Asset = new WeakReference<T>(asset);
            ReleaseAction = releaseAction;
        }
    }
    
    public abstract class AssetManager<TBaseObject>: IDisposable where TBaseObject: class
    {
        private Dictionary<string, Task> _assetLoadingTasks = new();
        private Dictionary<string, AssetHandle> _assetMap = new();

        public T Load<T>(string path) where T: TBaseObject 
        {
            if (TryGetAsset(path, out var obj) && obj is T t)
            {
                return t;
            }
            
            var handle = _Load<T>(path);
            _assetMap[path] = handle;
            handle.Asset.TryGetTarget(out t);
            return t;
        }

        protected abstract AssetHandle<T> _Load<T>(string path) where T : TBaseObject;

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

        protected abstract Task<AssetHandle<T>> _LoadAsync<T>(string path, CancellationToken token) where T : TBaseObject;
        
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