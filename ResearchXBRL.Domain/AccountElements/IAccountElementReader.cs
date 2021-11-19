using System;
using System.Collections.Generic;

namespace ResearchXBRL.Domain.AccountElements
{
    public interface IAccountElementReader : IDisposable
    {
        IEnumerable<AccountElement> Read();
    }
}
