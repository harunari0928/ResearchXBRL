namespace ResearchXBRL.Application.DTO.AccountItemReverseLookup;

public record ReverseLookupParameters
{
    public decimal SecuritiesCode { get; init; }
    public decimal NetSales { get; init; }
}
