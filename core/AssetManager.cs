using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace cfEngine.Asset
{
    public abstract class AssetManager
    {
        private Dictionary<string, Task> _assetLoadingTasks = new();
        private Dictionary<string, WeakReference<object>> _assetMap = new();

        public object Load(string path)
        {
            var obj = _Load(path);
            RegisterAsset(path, obj);
            return obj;
        }

        protected abstract object _Load(string path);
        
        public T Load<T>(string path) where T : class
        {
            var t = _Load<T>(path);
            RegisterAsset(path, t);
            return t;
        }

        protected abstract T _Load<T>(string path) where T : class;
        
        private void RegisterAsset(string path, object asset)
        {
            if (_assetMap.TryGetValue(path, out var wr))
            {
                if (wr.TryGetTarget(out _))
                {
                    throw new ArgumentException($"Repeated register of asset with key ({path})");
                }
            } 
            
            _assetMap[path] = new WeakReference<object>(asset);
        }

        protected bool TryGetAsset(string path, out object asset)
        {
            asset = null;
            return _assetMap.TryGetValue(path, out var wr) && wr.TryGetTarget(out asset);
        }
    }
}