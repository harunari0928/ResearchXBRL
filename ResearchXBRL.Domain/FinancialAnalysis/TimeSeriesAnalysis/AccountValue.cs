using System;
using ResearchXBRL.Domain.FinancialAnalysis.TimeSeriesAnalysis.AccountPeriods;

namespace ResearchXBRL.Domain.FinancialAnalysis.TimeSeriesAnalysis
{
    public class AccountValue
    {
        /// <summary>
        /// 決算期
        /// </summary>
        public IAccountsPeriod FinalAccountsPeriod { get; init; } = new InstantAccountPeriod();

        /// <summary>
        /// 金額
        /// </summary>
        public decimal Amount { get; init; }
    }
}
