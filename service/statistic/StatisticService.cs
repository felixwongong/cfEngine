#if CF_STATISTIC

namespace cfEngine.Service.Statistic
{
    public class StatisticService: IStatisticService
    {
        private readonly StatisticModel _model;
        IServiceModel IModelService.GetModel => _model;

        public StatisticService(StatisticModel model)
        {
            _model = model;
        }

        public void Record(string statisticKey)
        {
            var statistic = _model.GetOrCreateStatistic(statisticKey);
            statistic.RecordOnce();
        }

        public StatisticObjective CreateObjective(string regex, double start, double target = -1)
        {
            return new StatisticObjective(_model, regex, start, target);
        }

        public StatisticObjective CreateForwardObjective(string regex, double target = -1)
        {
            var matched = _model.GetMatchedStatistic(regex);

            double start = 0d;
            foreach (var (_, statistic) in matched)
            {
                start += statistic.Value;
            }

            return CreateObjective(regex, start, target);
        }

        public void Dispose()
        {
            _model.Dispose();
        }
    }
}

#endif