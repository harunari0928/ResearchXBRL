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

    public bool IsEmptyValue(in IndicatorType indicatorType)
    {
        if (!Exists(indicatorType))
        {
            throw new InvalidOperationException();
        }

        return !Indicators.Single(x => x.IndicatorType == IndicatorType.RateOfReturnOnEquitySummaryOfBusinessResults).Values.Any();
    }

    public void UpdateROEValue(in IReadOnlyDictionary<DateOnly, decimal> profitOrLossValues, in IReadOnlyDictionary<DateOnly, decimal> capitalValues)
    {
        indicators = indicators
            .Where(x => x.IndicatorType != IndicatorType.RateOfReturnOnEquitySummaryOfBusinessResults)
            .Append(new Indicator
            {
                IndicatorType = IndicatorType.RateOfReturnOnEquitySummaryOfBusinessResults,
                Values = CreateNewValues(profitOrLossValues, capitalValues)
            })
            .ToList();
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
