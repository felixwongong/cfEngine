using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cfEngine.Core;
using cfEngine.Service;

namespace cfEngine.Info
{
    public class InfoLayer: IService
    {
        private readonly Dictionary<Type, IInfoManager> _infoMap = new();
        public IReadOnlyDictionary<Type, IInfoManager> InfoMap => _infoMap;

        public void RegisterInfo(IInfoManager infoManager)
        {
            if (!_infoMap.TryAdd(infoManager.GetType(), infoManager))
            {
                throw new ArgumentException(nameof(infoManager), $"Info type {infoManager.GetType()} already exist");
            }
        }

        public IEnumerable<Task> LoadInfoAsync()
        {
            return InfoMap.Values.Select(info => info.LoadInfoAsync(Domain.TaskToken));
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
            foreach (var infoManager in _infoMap.Values)
            {
                infoManager.Dispose();
            }
            _infoMap.Clear();
        }
    }
}