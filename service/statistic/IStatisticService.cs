#if CF_STATISTIC

namespace cfEngine.Service.Statistic
{
    public interface IStatisticService: IService {
        public void Record(string statisticKey);
        public StatisticObjective CreateObjective(string regex, double start, double target = -1);
        public StatisticObjective CreateForwardObjective(string regex, double target = -1);
        
    }
}
#endif