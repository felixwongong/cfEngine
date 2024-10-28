#if CF_STATISTIC

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace cfEngine.Meta
{
    public class StatisticObjective: IDisposable
    {
        private readonly StatisticController _statisticController;
        public readonly string RegexKey;
        public readonly double Start;
        public readonly double Target;
        private double _value;
        public double Value => _value;
        
        public event Action<double> OnUpdated;
        public event Action OnCompleted;

        private HashSet<WeakReference<Statistic>> _statisticsRegistered = new();

        public StatisticObjective(StatisticController statisticController, string regexKey, double start, double target = -1) :
            this(regexKey, start, target)
        {
            _statisticController = statisticController;
            _statisticController.OnNewStatisticRecorded += OnNewStatisticRecorded;
        }
        
        private StatisticObjective(string regexKey, double start, double target = -1)
        {
            this.RegexKey = regexKey;
            this.Target = target;
            this.Start = start;
            this._value = 0;
        }

        private void OnNewStatisticRecorded(string statisticKey)
        {
            if (!Regex.IsMatch(statisticKey, RegexKey))
                return;
                
            var statistic = _statisticController.StatisticMap[statisticKey];
            var wr = new WeakReference<Statistic>(statistic);
            if (_statisticsRegistered.Contains(wr))
            {
                throw new ArgumentException("Statistic already in objective.");
            }
            
            _statisticsRegistered.Add(wr);
            statistic.OnUpdate += OnStatisticUpdate;
        }

        private void OnStatisticUpdate(double value)
        {
            List<WeakReference<Statistic>> pendingRemove = new();

            var totalValue = -Start;
            foreach (var wr in _statisticsRegistered)
            {
                if (!wr.TryGetTarget(out var statistic))
                {
                    pendingRemove.Add(wr);
                    continue;
                }

                totalValue += statistic.Value;
            }

            _value = totalValue;
            if (Target > 0 && _value > Target)
            {
                OnCompleted?.Invoke();
            }
            else
            {
                OnUpdated?.Invoke(_value);
            }
            
            foreach (var wr in pendingRemove)
            {
                _statisticsRegistered.Remove(wr);
            }
            pendingRemove.Clear();
        }
        
        public void Dispose()
        {
            if (_statisticController != null)
            {
                _statisticController.OnNewStatisticRecorded -= OnNewStatisticRecorded;
            }
            foreach (var wr in _statisticsRegistered)
            {
                if (wr.TryGetTarget(out var statistic))
                {
                    statistic.OnUpdate -= OnStatisticUpdate;
                }
            }
            
            _statisticsRegistered.Clear();
        }
    }
}

#endif