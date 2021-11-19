using System;

namespace ResearchXBRL.Domain.FinancialReports.Details
{
    public sealed class InstantPeriod : IPeriod
    {
        public DateTime InstantDateTime { get; init; }
    }
}
