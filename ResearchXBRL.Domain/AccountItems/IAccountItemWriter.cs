using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResearchXBRL.Domain.AccountItems
{
    public interface IAccountItemWriter : IDisposable
    {
        Task Write(IEnumerable<AccountItem> elements);
    }
}
