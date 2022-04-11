using System;

namespace ResearchXBRL.Domain.ImportFinancialReports.Contexts;

public sealed class InstantPeriod : IPeriod
{
    public DateTimeOffset InstantDate { get; init; }
}
