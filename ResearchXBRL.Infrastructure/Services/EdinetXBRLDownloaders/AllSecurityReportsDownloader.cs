using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using ResearchXBRL.Application.DTO.Results;

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

        protected override IAsyncEnumerable<IResult<DocumentInfo>> GetFilteredDocumentByIds(DateTime start, DateTime end)
        {
            return GetAllDocumentInfos(start, end)
                .Where(x =>
                {
                    if (x is Succeeded<DocumentInfo> succeeded)
                    {
                        return succeeded.Value.OrdinanceCode == "010";
                    }
                    return true; // NOTE: 失敗した結果はとりあえず返して呼び出し元でハンドリングする
                })
                .Where(x =>
                {
                    if (x is Succeeded<DocumentInfo> succeeded)
                    {
                        return succeeded.Value.FormCode == "030000"
                            || succeeded.Value.FormCode == "043000"
                            || succeeded.Value.FormCode == "050000";
                    }
                    return true; // NOTE: 失敗した結果はとりあえず返して呼び出し元でハンドリングする
                });
        }
    }
}
