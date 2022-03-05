using System;
using System.Collections.Generic;

namespace ResearchXBRL.Application.DTO.FinancialAnalysis.PerformanceIndicators.Indicators;

public sealed class Indicator
{
    public IndicatorType IndicatorType { get; init; }

    public IReadOnlyDictionary<DateOnly, decimal> Values { get; init; } = new Dictionary<DateOnly, decimal>();
}

public enum IndicatorType
{

    /// <summary>
    /// 売上高
    /// </summary>
    NetSales = 0,

    /// <summary>
    /// 営業利益
    /// </summary>
    OperatingIncome = 1,

    /// <summary>
    /// 経常利益
    /// </summary>
    OrdinaryIncome = 2,

    /// <summary>
    /// 親会社の所有者に帰属する利益
    /// </summary>
    ProfitLossAttributableToOwnersOfParent = 3,

    /// <summary>
    /// ROE
    /// </summary>
    RateOfReturnOnEquitySummaryOfBusinessResults = 4,

    /// <summary>
    /// １株当たり配当
    /// </summary>
    DividendPaidPerShareSummaryOfBusinessResults = 5
}
