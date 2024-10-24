using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using cfEngine.Core;
using Unity.VisualScripting;


namespace cfEngine.Core
{
    public partial class UserDataKey
    {
        public const string Statistic = "Statistic";
    }
}

namespace cfEngine.Meta.Statistic
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
    
    public class StatisticController: IDisposable, IRuntimeSavable
    {
        private Dictionary<string, Statistic> _statisticMap = new();
        public Dictionary<string, Statistic> StatisticMap => _statisticMap;

        public event Action<string> OnNewStatisticRecorded;

        public StatisticController()
        {
        }
        
        public void Initialize(IReadOnlyDictionary<string, JsonObject> dataMap)
        {
            if (dataMap.TryGetValue(UserDataKey.Statistic, out var data))
            {
                _statisticMap =  data.GetValue<Dictionary<string, Statistic>>();
            }
        }

        public void Save(Dictionary<string, object> dataMap)
        {
            dataMap[UserDataKey.Statistic] = _statisticMap;
        }
        
        public void Record(string statisticKey)
        {
            if (!_statisticMap.TryGetValue(statisticKey, out var statistic))
            {
                statistic = new Statistic();
                _statisticMap[statisticKey] = statistic;
                OnNewStatisticRecorded?.Invoke(statisticKey);
            }
            
            statistic.RecordOnce();
        }

        public StatisticObjective CreateObjective(string regex, double start, double target)
        {
            return new StatisticObjective(regex, start, target, this);
        }

        public void Dispose()
        {
            _statisticMap.Clear();
        }
    }
}