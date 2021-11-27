using System.Collections.Generic;
using ResearchXBRL.Application.DTO;

namespace ResearchXBRL.Application.Services
{
    public interface ITaxonomyDownloader
    {
        IAsyncEnumerable<EdinetTaxonomyData> Download();
    }
}
