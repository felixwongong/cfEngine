using System;
using System.Collections.Generic;

public class StatisticController
{
    private Dictionary<string, double> _statisticMap = new();
    public Dictionary<string, double> StatisticMap => _statisticMap;

    public void IncrementStatistic(string statisticKey)
    {
        if (_statisticMap.TryGetValue(statisticKey, out var value))
        {
            _statisticMap[statisticKey] = value + 1;
        }
        else
        {
            _statisticMap[statisticKey] = 1;
        }
    }
}