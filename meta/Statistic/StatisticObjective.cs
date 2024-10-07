
using System;

namespace cfEngine.Meta.Statistic
{
    public class StatisticObjective
    {
        public readonly string regex;
        public event Action OnStatisticUpdated;

        public StatisticObjective(string regex)
        {
            this.regex = regex;
        }

        internal void FireUpdate()
        {
            OnStatisticUpdated?.Invoke();
        }
    }
}

