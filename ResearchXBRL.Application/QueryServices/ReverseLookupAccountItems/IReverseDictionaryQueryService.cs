using System;
using System.Collections.Generic;
using ResearchXBRL.Application.DTO.ReverseLookupAccountItems;
using ResearchXBRL.Application.DTO.Results;

namespace ResearchXBRL.Application.QueryServices.ReverseLookupAccountItems;

public interface IReverseDictionaryQueryService
{
    IResult<IAsyncEnumerable<FinancialReport>> Get();
}
