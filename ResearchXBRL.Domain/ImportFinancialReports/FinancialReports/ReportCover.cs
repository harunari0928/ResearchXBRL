using System;

namespace ResearchXBRL.Domain.ImportFinancialReports.FinancialReports;

public sealed class ReportCover
{
    public string DocumentId { get; init; } = "";
    public string DocumentType { get; init; } = "";
    public string CompanyId { get; init; } = "";
    public string AccountingStandards { get; init; } = "";
    public DateTimeOffset SubmissionDate { get; init; }
}
