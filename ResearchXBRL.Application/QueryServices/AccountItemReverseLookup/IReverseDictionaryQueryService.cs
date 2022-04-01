using System;
using System.Collections.Generic;
using ResearchXBRL.Application.DTO.AccountItemReverseLookup;

namespace ResearchXBRL.Application.QueryServices.AccountItemReverseLookup;

public interface IReverseDictionaryQueryService
{
    IEnumerable<ReverseLookupParameters> Get();
}
