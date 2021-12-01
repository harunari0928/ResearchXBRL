using System;

namespace ResearchXBRL.Application.DTO.FinancialAnalysis.TimeSeriesAnalysis
{
    public class InstantPeriodViewModel : IAccountsPeriodViewModel
    {
        public DateTime Instant { get; init; }
    }
}
