using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

    public class StatisticController: IDisposable
    {
        private Dictionary<string, Statistic> _statisticMap = new();
        public Dictionary<string, Statistic> StatisticMap => _statisticMap;

        public event Action<string> OnNewStatisticRecorded; 

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