
using System;
using System.Collections.Generic;

namespace cfEngine.Meta.Statistic
{
    public class StatisticObjective: IDisposable
    {
        private readonly StatisticController _statisticController;
        public readonly string Regex;
        public readonly double Start;
        public readonly double Target;
        private double _value;
        public double Value => _value;
        
        public event Action<double> OnUpdated;

        private HashSet<WeakReference<Statistic>> _statisticsRegistered = new();

        public StatisticObjective(string regex, double start, double target, StatisticController statisticController) :
            this(regex, start, target)
        {
            _statisticController = statisticController;
            _statisticController.OnNewStatisticRecorded += OnNewStatisticRecorded;
        }
        
        private StatisticObjective(string regex, double start, double target)
        {
            this.Regex = regex;
            this.Target = target;
            this.Start = start;
            this._value = 0;
        }

        private void OnNewStatisticRecorded(string statisticKey)
        {
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
            OnUpdated?.Invoke(_value);
            
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

