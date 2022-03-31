using System.Collections.Generic;
using ResearchXBRL.Application.DTO;
using ResearchXBRL.Domain.AccountItems;

namespace ResearchXBRL.Application.Services
{
    public interface ITaxonomyParser
    {
        IEnumerable<AccountItem> Parse(EdinetTaxonomyData source);
    }
}
