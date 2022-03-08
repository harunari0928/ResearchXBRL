using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using ResearchXBRL.Application.DTO.FinancialAnalysis.PerformanceIndicators.Indicators;

namespace ResearchXBRL.Application.DTO.FinancialAnalysis.PerformanceIndicators;

public sealed class PerformanceIndicator
{
    private IReadOnlyList<Indicator> indicators = new List<Indicator>();
    public IReadOnlyList<Indicator> Indicators
    {
        get => indicators;
        init
        {
            indicators = value;
        }
    }

    public bool Exists(IndicatorType indicatorType)
    {
        return Indicators.Any(x => x.IndicatorType == indicatorType);
    }

    public void UpdateROEValue(in IReadOnlyDictionary<DateOnly, decimal> profitOrLossValues, in IReadOnlyDictionary<DateOnly, decimal> capitalValues)
    {
        if (!Exists(IndicatorType.RateOfReturnOnEquitySummaryOfBusinessResults))
        {
            throw new InvalidOperationException("業績指標内にROEを含めてください");
        }

        var oldROEValues = indicators
            .Single(x => x.IndicatorType == IndicatorType.RateOfReturnOnEquitySummaryOfBusinessResults)
            .Values;

        indicators = indicators
            .Where(x => x.IndicatorType != IndicatorType.RateOfReturnOnEquitySummaryOfBusinessResults)
            .Append(new Indicator
            {
                IndicatorType = IndicatorType.RateOfReturnOnEquitySummaryOfBusinessResults,
                Values = MergeValues(oldROEValues, CreateNewValues(profitOrLossValues, capitalValues))
            })
            .ToArray();
    }

    private static IReadOnlyDictionary<DateOnly, decimal> MergeValues(in IReadOnlyDictionary<DateOnly, decimal> oldValues, in IReadOnlyDictionary<DateOnly, decimal> calculatedValues)
    {
        var newValues = oldValues.ToDictionary(x => x.Key, y => y.Value);
        foreach (var value in calculatedValues)
        {
            if (oldValues.ContainsKey(value.Key))
            {
                continue;
            }

            newValues.Add(value.Key, value.Value);
        }
        return newValues;
    }
    private static Dictionary<DateOnly, decimal> CreateNewValues(in IReadOnlyDictionary<DateOnly, decimal> profitOrLossValues, in IReadOnlyDictionary<DateOnly, decimal> capitalValues)
    {
        var newValues = new Dictionary<DateOnly, decimal>();
        foreach (var profitOrLossWithDate in profitOrLossValues)
        {
            if (!capitalValues.ContainsKey(profitOrLossWithDate.Key))
            {
                continue;
            }

            var capitalValue = capitalValues[profitOrLossWithDate.Key];
            newValues.Add(profitOrLossWithDate.Key, profitOrLossWithDate.Value / capitalValue);
        }

        return newValues;
    }
}
