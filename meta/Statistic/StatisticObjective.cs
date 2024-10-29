#if CF_STATISTIC

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using cfEngine.Logging;

namespace cfEngine.Meta
{
    [Serializable]
    public class StatisticObjective: IDisposable
    {
        /// <summary>
        /// default to Game Statistic, create a Controller property with new keyword if want custom statistic controller
        /// </summary>
        protected StatisticController Controller => Game.Meta?.Statistic;
        
        public readonly string Regex;
        public readonly double Start;
        public readonly double Target;
        [JsonInclude] [JsonPropertyName(nameof(Value))]
        private double _value;
        [JsonIgnore]
        public double Value => _value;
        
        public event Action<double> OnUpdated;
        public event Action OnCompleted;

        private HashSet<WeakReference<Statistic>> _statisticsRegistered = new();

        public StatisticObjective(string regex, double start, double target = -1)
        {
            this.Regex = regex;
            this.Target = target;
            this.Start = start;
            this._value = 0;

            if (Controller == null)
            {
                Log.LogError("Statistic Controller is null");
                return;
            }

            Controller.OnNewStatisticRecorded += OnNewStatisticRecorded;
        }

        ~StatisticObjective()
        {
            Log.LogWarning($"Objective ({Regex}) not disposed properly");
            Dispose();
        }

        private void OnNewStatisticRecorded(string statisticKey)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(statisticKey, Regex))
                return;
                
            var statistic = Controller.StatisticMap[statisticKey];
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
            if (Controller != null)
            {
                Controller.OnNewStatisticRecorded -= OnNewStatisticRecorded;
            }

            if (_statisticsRegistered != null)
            {
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
}

#endif