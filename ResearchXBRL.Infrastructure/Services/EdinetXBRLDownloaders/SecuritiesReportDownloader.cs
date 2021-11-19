using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace ResearchXBRL.Infrastructure.Services.EdinetXBRLDownloaders
{
    public sealed class SecuritiesReportDownloader : EdinetXBRLDownloader
    {
        public SecuritiesReportDownloader(IHttpClientFactory httpClientFactory, string apiVersion) : base(httpClientFactory, apiVersion)
        {            
        }

        protected override IAsyncEnumerable<string> GetFilteredDocumentIds(DateTime start, DateTime end)
        {
            return GetAllDocumentInfos(start, end)
                // 有価証券報告書のみを絞り込む
                .Where(x => x.OrdinanceCode == "010")
                .Where(x => x.FormCode == "030000")
                .Select(x => x.DocID);
        }
    }
}
