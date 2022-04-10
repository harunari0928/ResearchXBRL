using System;

namespace ResearchXBRL.Application.DTO.ReverseLookupAccountItems;

public record ReverseLookupResult(string NormalizedName, string OriginalName, int SecuritiesCode, DateOnly FiscalYear);
