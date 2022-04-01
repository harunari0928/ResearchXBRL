using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResearchXBRL.Domain.AccountItemReverseLookup.AccountItems;

public interface IAccountItemRepository
{
    ValueTask Add(IEnumerable<AccountItem> normalizedAccountItemNames);
}