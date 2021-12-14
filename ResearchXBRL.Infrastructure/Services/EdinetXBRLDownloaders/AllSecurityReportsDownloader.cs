using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace ResearchXBRL.Infrastructure.Services.EdinetXBRLDownloaders
{
    /// <summary>
    /// 有価証券報告書、四半期報告書、半期報告書をダウンロードする
    /// </summary>
    public sealed class AllSecurityReportsDownloader : EdinetXBRLDownloader
    {
        public AllSecurityReportsDownloader(IHttpClientFactory httpClientFactory, string apiVersion) : base(httpClientFactory, apiVersion)
        {
        }

        protected override IAsyncEnumerable<DocumentInfo> GetFilteredDocumentIds(DateTime start, DateTime end)
        {
            return GetAllDocumentInfos(start, end)
                .Where(x => x.OrdinanceCode == "010"
                    || x.OrdinanceCode == "140"
                    || x.OrdinanceCode == "160")
                .Where(x =>
                    x.FormCode == "030000");
        }
    }
}
