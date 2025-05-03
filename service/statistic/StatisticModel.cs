using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using cfEngine.Core;

namespace cfEngine.Core
{
    public partial class UserDataKey
    {
        public const string Statistic = "Statistic";
    }
}

namespace cfEngine.Service.Statistic
{
    public class Statistic
    {
        private double _value;
        public double Value => _value;
        public event Action<double> OnUpdate;

        public void RecordOnce()
        {
            _value += 1;
            OnUpdate?.Invoke(_value);
        }
    }

    public class StatisticModel: IServiceModel
    {
        private Dictionary<string, Statistic> _statisticMap = new();
        public Dictionary<string, Statistic> StatisticMap => _statisticMap;
        
        public event Action<string> OnNewStatisticRecorded;
        
        public void Initialize(IUserData userData)
        {
            if (!userData.TryGetContext(UserDataKey.Statistic, out _statisticMap))
            {
                _statisticMap = new Dictionary<string, Statistic>();
            }
        }

        public void SetSaveData(Dictionary<string, object> dataMap)
        {
            dataMap[UserDataKey.Statistic] = _statisticMap;
        }
        
        public bool TryGetStat(string key, out Statistic statistic)
        {
            return _statisticMap.TryGetValue(key, out statistic);
        }
        
        public Statistic GetOrCreateStatistic(string statisticKey)
        {
            if (!_statisticMap.TryGetValue(statisticKey, out var statistic))
            {
                statistic = new Statistic();
                _statisticMap[statisticKey] = statistic;
                OnNewStatisticRecorded?.Invoke(statisticKey);
            }

            return statistic;
        }
        
        public IEnumerable<KeyValuePair<string, Statistic>> GetMatchedStatistic(string regex)
        {
            return _statisticMap.Where(kvp => Regex.IsMatch(kvp.Key, regex));
        }
        
        public void Dispose()
        {
            _statisticMap.Clear();
        }
    }
}