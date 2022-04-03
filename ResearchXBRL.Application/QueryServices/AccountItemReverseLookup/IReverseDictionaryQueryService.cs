using System;
using System.Collections.Generic;
using ResearchXBRL.Application.DTO.AccountItemReverseLookup;
using ResearchXBRL.Application.DTO.Results;

namespace ResearchXBRL.Application.QueryServices.AccountItemReverseLookup;

public interface IReverseDictionaryQueryService
{
    IResult<IAsyncEnumerable<FinancialReport>> Get();
}
