using System;

namespace ResearchXBRL.Domain.FinancialReports
{
    public sealed class ReportCover
    {
        public string DocumentTitle { get; init; }
        public string CompanyName { get; init; }
        public DateTimeOffset SubmissionDate { get; init; }
    }
}
