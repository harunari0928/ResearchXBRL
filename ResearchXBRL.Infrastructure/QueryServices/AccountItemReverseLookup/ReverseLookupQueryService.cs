using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ResearchXBRL.Application.DTO.AccountItemReverseLookup;
using ResearchXBRL.Application.QueryServices.AccountItemReverseLookup;
using ResearchXBRL.Infrastructure.Shared;

namespace ResearchXBRL.Infrastructure.Services.AccountItemReverseLookup;

public class ReverseLookupQueryService : SQLService, IReverseLookupQueryService
{
    public ValueTask<IReadOnlyCollection<ReverseLookupResult>> Lookup(FinancialReport financialReport)
    {
        throw new NotImplementedException();
    }
}
