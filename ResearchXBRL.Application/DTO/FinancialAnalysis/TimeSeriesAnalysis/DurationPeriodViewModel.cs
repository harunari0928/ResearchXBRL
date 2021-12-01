using System;

namespace ResearchXBRL.Application.DTO.FinancialAnalysis.TimeSeriesAnalysis
{
    public class DurationPeriodViewModel : IAccountsPeriodViewModel
    {
        public DateTime From { get; init; }
        public DateTime To { get; init; }
    }
}
