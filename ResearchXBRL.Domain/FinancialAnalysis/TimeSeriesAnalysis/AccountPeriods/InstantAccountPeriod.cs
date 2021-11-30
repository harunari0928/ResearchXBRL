using System.Reflection.PortableExecutable;
using System;

namespace ResearchXBRL.Domain.FinancialAnalysis.TimeSeriesAnalysis.AccountPeriods
{
    /// <summary>
    /// 貸借対照表項目の決算期
    /// </summary>
    public sealed class InstantAccountPeriod : IAccountsPeriod
    {
        public DateTime Instant { get; init; }
    }
}
