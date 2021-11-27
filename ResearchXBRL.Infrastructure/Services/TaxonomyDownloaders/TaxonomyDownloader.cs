using System;
using System.Collections.Generic;
using System.Net.Http;
using ResearchXBRL.Application.DTO;
using ResearchXBRL.Application.Services;

namespace ResearchXBRL.Infrastructure.Services.TaxonomyDownloaders
{
    public class TaxonomyDownloader : ITaxonomyDownloader
    {
        private readonly HttpClient httpClient;

        public TaxonomyDownloader(IHttpClientFactory httpClientFactory)
        {
            httpClient = httpClientFactory.CreateClient(typeof(TaxonomyDownloader).Name);
        }

        public IAsyncEnumerable<EdinetTaxonomyData> Download()
        {
            throw new NotImplementedException();
        }
    }
}
