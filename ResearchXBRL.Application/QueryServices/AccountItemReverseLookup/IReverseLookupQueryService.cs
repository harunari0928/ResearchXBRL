using System.Collections.Generic;
using System.Threading.Tasks;
using ResearchXBRL.Application.DTO.AccountItemReverseLookup;

namespace ResearchXBRL.Application.QueryServices.AccountItemReverseLookup;

public interface IReverseLookupQueryService
{
    ValueTask<IReadOnlyCollection<ReverseLookupResult>> Lookup(ReverseLookupParameters parameters);
}
