using System.Linq;
using System.IO;
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
        private readonly IFileStorage storage;

        public TaxonomyDownloader(
            IHttpClientFactory httpClientFactory,
            IFileStorage storage)
        {
            httpClient = httpClientFactory.CreateClient(typeof(TaxonomyDownloader).Name);
            this.storage = storage;
        }

        public async IAsyncEnumerable<EdinetTaxonomyData> Download()
        {
            using var responseMessage = await httpClient.GetAsync(TaxonomyFileUrl);
            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new HttpRequestException("Taxonomyダウンロード失敗", null, responseMessage.StatusCode);
            }

            var stream = await responseMessage.Content.ReadAsStreamAsync();
            storage.Set(stream, "/work");
            storage.Unzip("/work/data.zip", "/unzipped");
            var basePath = "/unzipped/data/EDINET/taxonomy";
            foreach (var taxonomyVersion in storage.GetFolderNames(basePath))
            {
                foreach (var classification in new string[] { "jpcor", "jppfs", "jpigp" })
                {
                    if (!storage.GetFolderNames(Path.Combine(basePath, taxonomyVersion, "/taxonomy/"))
                        .Contains(classification))
                    {
                        continue;
                    }

                    yield return new EdinetTaxonomyData
                    {
                        TaxonomyVersion = DateTime.Parse(taxonomyVersion),
                        Classification = classification,
                        SchemaDataStream = GetSchemaStream(basePath, taxonomyVersion, classification),
                        LabelDataStream = GetLabelStream(basePath, taxonomyVersion, classification)
                    };
                }
            }
        }

        private Stream GetSchemaStream(string basePath, string version, string classification)
        {
            return storage.Get(Path.Combine(basePath, version, $"/taxonomy/{classification}", version, $"{classification}_cor_{version}.xsd"));
        }
        private Stream GetLabelStream(string basePath, string version, string classification)
        {
            return storage.Get(Path.Combine(basePath, version, $"/taxonomy/{classification}", version, $"label/{classification}_{version}_lab.xml"));
        }

        private const string TaxonomyFileUrl
            = "http://lang.main.jp/xbrl/data.zip";
    }
}
