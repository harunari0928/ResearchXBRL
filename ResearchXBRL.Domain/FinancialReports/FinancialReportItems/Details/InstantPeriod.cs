using System;

namespace ResearchXBRL.Domain.FinancialReports.FinancialReportItems.Details
{
    public sealed class InstantPeriod : IPeriod
    {
        public DateTime InstantDateTime { get; init; }
    }
}
