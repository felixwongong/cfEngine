#if CF_STATISTIC

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using cfEngine.Core;

namespace cfEngine.Core
{
    using Service.Statistic;
    
    public partial class UserDataKey
    {
        public const string Statistic = "Statistic";
    }
    
    public static partial class ServiceName
    {
        public const string Statistic = "Statistic";
    }
    
    public static partial class GameExtension
    {
        public static Game WithStatistic(this Game game, IStatisticService service)
        {
            game.Register(service, ServiceName.Statistic);
            return game;
        }
        
        public static StatisticService GetStatistic(this Game game) => game.GetService<StatisticService>(ServiceName.Statistic);
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
    
    public interface IStatisticService: IService {
        public Dictionary<string, Statistic> StatisticMap { get; }
        public void Record(string statisticKey);
        public StatisticObjective CreateObjective(string regex, double start, double target = -1);
        public StatisticObjective CreateForwardObjective(string regex, double target = -1);
        
        public event Action<string> OnNewStatisticRecorded;
    }

    public class StatisticService: IStatisticService, IRuntimeSavable
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

        public StatisticObjective CreateObjective(string regex, double start, double target = -1)
        {
            return new StatisticObjective(this, regex, start, target);
        }

        public StatisticObjective CreateForwardObjective(string regex, double target = -1)
        {
            var matched = _statisticMap
                .Where(kvp => Regex.IsMatch(kvp.Key, regex));

            double start = 0d;
            foreach (var (_, statistic) in matched)
            {
                start += statistic.Value;
            }

            return CreateObjective(regex, start, target);
        }

        public void Dispose()
        {
            _statisticMap.Clear();
        }
    }
}

#endif