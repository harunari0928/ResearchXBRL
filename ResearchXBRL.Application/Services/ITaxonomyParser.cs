using System.Collections.Generic;
using ResearchXBRL.Application.DTO;
using ResearchXBRL.Domain.ImportAccountItems.AccountItems;

namespace ResearchXBRL.Application.Services
{
    public interface ITaxonomyParser
    {
        IEnumerable<AccountItem> Parse(EdinetTaxonomyData source);
    }
}
