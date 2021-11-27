using System.Collections.Generic;
using ResearchXBRL.Application.DTO;
using ResearchXBRL.Domain.AccountElements;

namespace ResearchXBRL.Application.Services
{
    public interface ITaxonomyParser
    {
        IEnumerable<AccountElement> Read(EdinetTaxonomyData source);
    }
}
