using System;

namespace ResearchXBRL.Application.DTO.FinancialAnalysis.TimeSeriesAnalysis
{
    public sealed class AccountsPeriodViewModel
    {
        public DateTime? Instant { get; init; }
        public DateTime? From { get; init; }
        public DateTime? To { get; init; }
    }
}
