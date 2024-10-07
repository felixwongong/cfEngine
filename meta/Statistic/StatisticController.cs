using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace cfEngine.Meta.Statistic
{
    public class StatisticController
    {
        public class Statistic
        {
            private double _value;
            public double Value => _value;
            private List<WeakReference<StatisticObjective>> _objectiveReferences = new();

            public void RecordOnce()
            {
                _value += 1;
                for (var i = _objectiveReferences.Count - 1; i >= 0; i--)
                {
                    if (!_objectiveReferences[i].TryGetTarget(out var objective))
                    {
                        _objectiveReferences.RemoveAt(i);
                        continue;
                    }

                    objective.FireUpdate();
                }
            }

            public void RegisterObjective(StatisticObjective objective)
            {
                _objectiveReferences.Add(new WeakReference<StatisticObjective>(objective));
            }
        }

        private List<WeakReference<StatisticObjective>> _objectives = new();
        private Dictionary<string, Statistic> _statisticMap = new();
        public Dictionary<string, Statistic> StatisticMap => _statisticMap;

        public void IncrementStatistic(string statisticKey)
        {
            if (_statisticMap.TryGetValue(statisticKey, out var statistic))
            {
                statistic.RecordOnce();
            }
            else
            {
                statistic = new Statistic();
                for (var i = _objectives.Count - 1; i >= 0; i--)
                {
                    if (!_objectives[i].TryGetTarget(out var objective))
                    {
                        _objectives.RemoveAt(i);
                        continue;
                    }

                    if (Regex.IsMatch(objective.regex, statisticKey))
                    {
                        statistic.RegisterObjective(objective);
                    }
                }

                _statisticMap[statisticKey] = statistic;
            }
        }

        public StatisticObjective CreateObjective(string regex)
        {
            var objective = new StatisticObjective(regex);

            _objectives.Add(new WeakReference<StatisticObjective>(objective));
            
            foreach (var (statisticKey, statistic) in _statisticMap)
            {
                if(!Regex.IsMatch(statisticKey, regex)) continue;
                
                statistic.RegisterObjective(objective);
            }

            return objective;
        }
    }
}