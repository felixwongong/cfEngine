using System;
using System.Collections.Generic;
using cfEngine.Info;
using cfEngine.Serialize;

namespace cfEngine.Core.Layer
{
    public class InfoLayer
    {
        private readonly string infoRootPath;
        private readonly StreamSerializer _serializer;

        private Dictionary<Type, InfoManager> infoMap = new();

        public InfoLayer(string infoRootPath, StreamSerializer serializer)
        {
            this.infoRootPath = infoRootPath;

            if (_serializer != null)
            {
                throw new InvalidOperationException("StreamSerializer already exist");
            }

            _serializer = serializer;
        }

        public void RegisterInfo(InfoManager infoManager)
        {
            var infoKey = infoManager.InfoDirectory;

            if (string.IsNullOrEmpty(infoKey))
            {
                throw new ArgumentNullException(nameof(infoManager.InfoDirectory),
                    $"{nameof(infoManager)} infoDict key is invalid.");
            }

            if (!infoMap.TryAdd(infoManager.GetType(), infoManager))
            {
                throw new ArgumentException(nameof(infoManager), $"Info key {infoKey} already exist");
            }

            infoManager.Serializer = _serializer;
            infoManager.InfoRoot = infoRootPath;
        }

        public bool TryGetInfo<TInfo>(out TInfo infoManager) where TInfo : InfoManager
        {
            if (infoMap.TryGetValue(typeof(TInfo), out var info) && info is TInfo tInfo)
            {
                infoManager = tInfo;
                return true;
            }

            infoManager = null;
            return false;
        }

        public TInfo GetInfo<TInfo>() where TInfo : InfoManager
        {
            return infoMap[typeof(TInfo)] as TInfo;
        }
    }
}