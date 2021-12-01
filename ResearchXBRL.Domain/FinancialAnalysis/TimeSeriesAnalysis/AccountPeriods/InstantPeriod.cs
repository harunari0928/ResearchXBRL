using System.Reflection.PortableExecutable;
using System;

namespace ResearchXBRL.Domain.FinancialAnalysis.TimeSeriesAnalysis.AccountPeriods
{
    /// <summary>
    /// 貸借対照表項目の決算期
    /// </summary>
    public sealed class InstantPeriod : IAccountsPeriod
    {
        public DateTime Instant { get; init; }
    }
}
