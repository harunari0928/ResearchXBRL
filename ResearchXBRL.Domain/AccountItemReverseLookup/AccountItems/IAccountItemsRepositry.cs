using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResearchXBRL.Domain.AccountItemReverseLookup.AccountItems;

public interface IAccountItemsRepository
{
    ValueTask Add(IAsyncEnumerable<AccountItem> normalizedAccountItems);
}