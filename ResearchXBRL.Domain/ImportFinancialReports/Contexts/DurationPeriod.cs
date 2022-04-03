using System;

namespace ResearchXBRL.Domain.ImportFinancialReports.Contexts;

public sealed class DurationPeriod : IPeriod
{
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}
