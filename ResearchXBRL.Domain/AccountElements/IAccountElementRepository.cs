using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResearchXBRL.Domain.AccountElements
{
    public interface IAccountElementRepository : IAccountElementReader, IAccountElementWriter
    {
    }
}
