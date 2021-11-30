using System;

namespace ResearchXBRL.Domain.FinancialAnalysis.TimeSeriesAnalysisResults.AccountPeriods
{
    /// <summary>
    /// 損益計算書の決算期
    /// </summary>
    public sealed class DurationPeriod : IAccountsPeriod
    {
        public DateTime From { get; init; }
        public DateTime To { get; init; }
    }
}
