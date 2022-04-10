using System;
using System.Collections.Generic;

namespace ResearchXBRL.Application.DTO.ReverseLookupAccountItems;

public record FinancialReport
{
    public decimal SecuritiesCode { get; init; }
    public AccountingStandards AccountingStandard { get; init; }
    public DateOnly FiscalYear { get; init; }
    public IReadOnlyDictionary<string, decimal?> AccountAmounts { get; init; } = new Dictionary<string, decimal?>();
}

public enum AccountingStandards
{
    Japanese = 1
}
