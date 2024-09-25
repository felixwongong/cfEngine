using System;
using System.Collections.Generic;

namespace cfEngine.Asset
{
    public abstract class AssetManager
    {
        private Dictionary<string, WeakReference<object>> _assetMap = new();
        protected IReadOnlyDictionary<string, WeakReference<object>> AssetMap => _assetMap;

        public abstract object Load(string path);
        public abstract T Load<T>(string path) where T : class;

        protected void Register(string path, object asset)
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