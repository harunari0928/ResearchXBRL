using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResearchXBRL.Domain.AccountElements
{
    public interface IAccountElementWriter : IDisposable
    {
        Task Write(IEnumerable<AccountElement> elements);
    }
}
