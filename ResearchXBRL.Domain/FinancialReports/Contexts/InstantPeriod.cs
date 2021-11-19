using System;

namespace ResearchXBRL.Domain.FinancialReports.Contexts
{
    public sealed class InstantPeriod : IPeriod
    {
        public DateTimeOffset InstantDateTime { get; init; }
    }
}
