namespace ResearchXBRL.Application.DTO.AccountItemReverseLookup;

public record ReverseLookupResult
{
    public string NormalizedName { get; init; } = "";
    public string OriginalName { get; init; } = "";
}

