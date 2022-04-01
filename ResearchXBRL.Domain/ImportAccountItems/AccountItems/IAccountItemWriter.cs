using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResearchXBRL.Domain.ImportAccountItems.AccountItems;

public interface IAccountItemsWriter : IDisposable
{
    Task Write(IEnumerable<AccountItem> elements);
}
