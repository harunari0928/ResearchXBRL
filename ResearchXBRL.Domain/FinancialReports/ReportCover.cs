using System;

namespace ResearchXBRL.Domain.FinancialReports
{
    public sealed class ReportCover
    {
        public string DocumentId { get; init; }
        public string DocumentType { get; init; }
        public string CompanyId { get; init; }
        public DateTimeOffset SubmissionDate { get; init; }
    }
}
