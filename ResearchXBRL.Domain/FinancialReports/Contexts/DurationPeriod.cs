using System;

namespace ResearchXBRL.Domain.FinancialReports.Contexts
{
    public sealed class DurationPeriod : IPeriod
    {
        public DateTimeOffset From { get; init; }
        public DateTimeOffset To { get; init; }
    }
}
