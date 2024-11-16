using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using cfEngine.Logging;

namespace cfEngine.Asset
{
    public abstract class AssetHandle
    {
        public readonly Action ReleaseAction;
        public AssetHandle(Action releaseAction)
        {
            ReleaseAction = releaseAction;
        } 
    }
    public class AssetHandle<T>: AssetHandle where T : class
    {
        public readonly WeakReference<T> Asset;

        public AssetHandle(T asset, Action releaseAction): base(releaseAction)
        {
            Asset = new WeakReference<T>(asset);
        }
    }
    
    public abstract class AssetManager<TBaseObject>: IDisposable where TBaseObject: class
    {
        private Dictionary<string, Task> _assetLoadingTasks = new();
        private Dictionary<string, AssetHandle> _assetMap = new();

        public T Load<T>(string path) where T: class, TBaseObject 
        {
            if (TryGetAsset<T>(path, out var t))
            {
                return t;
            }
            
            var handle = _Load<T>(path);
            _assetMap[path] = handle;
            handle.Asset.TryGetTarget(out t);
            return t;
        }

        protected abstract AssetHandle<T> _Load<T>(string path) where T : class, TBaseObject;

        public async Task<T> LoadAsync<T>(string path, CancellationToken token = default) where T: class, TBaseObject
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

            _assetMap[path] = result;
            result.Asset.TryGetTarget(out var asset);
            return asset;
        }

        protected abstract Task<AssetHandle<T>> _LoadAsync<T>(string path, CancellationToken token = default) where T : class, TBaseObject;
        
        public bool TryGetAsset<T>(string path, out T asset) where T: class, TBaseObject
        {
            asset = null;
            return _assetMap.TryGetValue(path, out var handle) && 
                   handle is AssetHandle<T> tHandle &&
                   tHandle.Asset.TryGetTarget(out asset);
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
                wr.ReleaseAction?.Invoke();
            }
            
            _assetMap.Clear();
        }
    }
}
