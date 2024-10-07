using System;
using System.Collections.Generic;
using cfEngine.Info;
using cfEngine.IO;
using cfEngine.Serialize;
using CofyDev.Xml.Doc;

namespace cfEngine.Core.Layer
{
    public class InfoLayer<TStorage, TSerializer>: IDisposable where TStorage: Storage where TSerializer: Serializer
    {
        private readonly TStorage _storage;
        private readonly TSerializer _serializer;

        private readonly Dictionary<Type, InfoManager> _infoMap = new();
        public IReadOnlyDictionary<Type, InfoManager> InfoMap => _infoMap;

        public InfoLayer(TStorage storage, TSerializer serializer)
        {
            _storage = storage;
            _serializer = serializer;
        }

        public void RegisterInfo(InfoManager infoManager)
        {
            var infoKey = infoManager.InfoDirectory;

            if (string.IsNullOrEmpty(infoKey))
            {
                throw new ArgumentNullException(nameof(infoManager.InfoDirectory),
                    $"{nameof(infoManager)} ValueMap key is invalid.");
            }

            if (!_infoMap.TryAdd(infoManager.GetType(), infoManager))
            {
                throw new ArgumentException(nameof(infoManager), $"Info key {infoKey} already exist");
            }

            infoManager.Serializer = _serializer;
            infoManager.Storage = _storage;
            infoManager.Encoder = new DataObjectEncoder();
        }

        public bool TryGetInfo<TInfo>(out TInfo infoManager) where TInfo : InfoManager
        {
            if (_infoMap.TryGetValue(typeof(TInfo), out var info) && info is TInfo tInfo)
            {
                infoManager = tInfo;
                return true;
            }

            infoManager = null;
            return false;
        }

        public TInfo Get<TInfo>() where TInfo : InfoManager
        {
            return _infoMap[typeof(TInfo)] as TInfo;
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