using System.Linq;
using System.IO;
using System;
using System.Collections.Generic;
using System.Net.Http;
using ResearchXBRL.Application.DTO;
using ResearchXBRL.Application.Services;
using ResearchXBRL.Infrastructure.Shared.FileStorages;

namespace ResearchXBRL.Infrastructure.Services.TaxonomyDownloaders
{
    public class TaxonomyDownloader : ITaxonomyDownloader, IDisposable
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
            storage.Set(stream, "/work/data.zip");
            storage.Unzip("/work/data.zip", "/unzipped");
            var basePath = "/unzipped/data/EDINET/taxonomy";
            foreach (var taxonomyVersion in storage.GetDirectoryNames(basePath))
            {
                foreach (var classification in new string[] { "jppfs", "jpigp" })
                {
                    if (!storage.GetDirectoryNames($"{basePath}/{taxonomyVersion}/taxonomy/")
                        .Contains(classification))
                    {
                        continue;
                    }

                    if (!storage.GetDirectoryNames($"{basePath}/{taxonomyVersion}/taxonomy/{classification}")
                        .Contains(taxonomyVersion))
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
            return storage.Get($"{basePath}/{version}/taxonomy/{classification}/{version}/{classification}_cor_{version}.xsd")
                ?? throw new FileNotFoundException("インポート対象ファイルが存在しません");
        }
        private Stream GetLabelStream(string basePath, string version, string classification)
        {
            return storage.Get($"{basePath}/{version}/taxonomy/{classification}/{version}/label/{classification}_{version}_lab.xml")
                ?? throw new FileNotFoundException("インポート対象ファイルが存在しません");
        }

        public void Dispose()
        {
            foreach (var directory in new string[] { "work", "unzipped" }
                .Intersect(storage.GetDirectoryNames(".")))
            {
                storage.Delete(directory);
            }
        }

        private const string TaxonomyFileUrl
            = "http://lang.main.jp/xbrl/data.zip";
    }
}
