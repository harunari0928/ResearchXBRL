using System;

namespace ResearchXBRL.Domain.FinancialReports.Details
{
    public sealed class DurationPeriod : IPeriod
    {
        public DateTime From { get; init; }
        public DateTime To { get; init; }
    }
}
