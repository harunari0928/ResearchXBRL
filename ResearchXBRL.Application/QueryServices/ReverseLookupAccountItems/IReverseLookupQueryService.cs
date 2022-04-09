using System.Collections.Generic;
using System.Threading.Tasks;
using ResearchXBRL.Application.DTO.ReverseLookupAccountItems;

namespace ResearchXBRL.Application.QueryServices.ReverseLookupAccountItems;

public interface IReverseLookupQueryService
{
    ValueTask<IReadOnlyCollection<ReverseLookupResult>> Lookup(FinancialReport financialReport);
}
