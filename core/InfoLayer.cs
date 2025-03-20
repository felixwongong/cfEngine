using System;
using System.Collections.Generic;
using cfEngine.Info;
using cfEngine.IO;
using cfEngine.Serialize;
using CofyDev.Xml.Doc;

namespace cfEngine.Core.Layer
{
    public partial class InfoLayer: IDisposable
    {
        private readonly IStorage _storage;
        private readonly ISerializer _serializer;

        private readonly Dictionary<string, IInfoManager> _infoMap = new();
        public IReadOnlyDictionary<string, IInfoManager> InfoMap => _infoMap;

        public InfoLayer(IStorage storage, ISerializer serializer)
        {
            _storage = storage;
            _serializer = serializer;
        }

        public void RegisterInfo(IInfoManager infoManager)
        {
            var infoKey = infoManager.InfoDirectory;

            if (string.IsNullOrEmpty(infoKey))
            {
                throw new ArgumentNullException(nameof(infoManager.InfoDirectory),
                    $"{nameof(infoManager)} ValueMap key is invalid.");
            }

            if (!_infoMap.TryAdd(infoManager.infoKey, infoManager))
            {
                throw new ArgumentException(nameof(infoManager), $"Info key {infoKey} already exist");
            }

            infoManager.Serializer = _serializer;
            infoManager.Storage = _storage;
            infoManager.Encoder = new DataObjectEncoder();
        }

        public bool TryGetInfo<TInfo>(string infoKey, out TInfo infoManager) where TInfo : InfoManager
        {
            if (_infoMap.TryGetValue(infoKey, out var info) && info is TInfo tInfo)
            {
                infoManager = tInfo;
                return true;
            }

            infoManager = null;
            return false;
        }

        public bool TryGetInfoByName<TInfo>(out TInfo infoManager) where TInfo : InfoManager
        {
            return TryGetInfo(nameof(TInfo), out infoManager);
        }

        public TInfo Get<TInfo>(string infoKey) where TInfo : InfoManager
        {
            return _infoMap[infoKey] as TInfo;
        }

        public TInfo Get<TInfo>() where TInfo : InfoManager
        {
            return Get<TInfo>(nameof(TInfo));
        }

        public void Dispose()
        {
            _storage.Dispose();
            foreach (var infoManager in _infoMap.Values)
            {
                infoManager.Dispose();
            }
            _infoMap.Clear();
        }
    }
}