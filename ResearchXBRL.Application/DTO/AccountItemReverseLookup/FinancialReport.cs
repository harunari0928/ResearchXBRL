using System;

namespace ResearchXBRL.Application.DTO.AccountItemReverseLookup;

public record FinancialReport
{
    public decimal SecuritiesCode { get; init; }
    public AccountingStandards AccountingStandard { get; init; }
    public DateOnly FiscalYear { get; init; }
    public decimal? NetSales { get; init; }
}

public enum AccountingStandards
{
    Japanese = 1
}
