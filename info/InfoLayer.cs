using System;
using System.Collections.Generic;
using cfEngine.IO;
using cfEngine.Serialize;
using cfEngine.Service;
using CofyDev.Xml.Doc;

namespace cfEngine.Core
{
    using Info;
    public static partial class ServiceName
    {
        public const string Info = "Info";
    }
    
    public static partial class GameExtension
    {
        public static Game WithInfo(this Game game, InfoLayer service)
        {
            game.Register(service, ServiceName.Info);
            return game;
        }
        
        public static InfoLayer GetInfo(this Game game) => game.GetService<InfoLayer>(ServiceName.Info);
    }
}

namespace cfEngine.Info
{
    public class InfoLayer: IService
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
            var infoKey = infoManager.infoDirectory;

            if (string.IsNullOrEmpty(infoKey))
            {
                throw new ArgumentNullException(nameof(infoManager.infoDirectory),
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