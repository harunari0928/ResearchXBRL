using ResearchXBRL.Application.Services;
using ResearchXBRL.Application.Usecase.FinancialReports;
using ResearchXBRL.Domain.FinancialReports;
using System;
using System.Collections.Generic;

namespace ResearchXBRL.Application.FinancialReports
{
    public sealed class DownloadFinancialReportsInteractor : IDownloadFinancialReporsUsecase
    {
        private readonly IEdinetXBRLDownloader downloader;
        private readonly IEdinetXBRLParser parser;

        public DownloadFinancialReportsInteractor(
            IEdinetXBRLDownloader downloader,
            IEdinetXBRLParser parser)
        {
            this.downloader = downloader;
            this.parser = parser;
        }

        public async IAsyncEnumerable<FinancialReport> Handle(DateTimeOffset start, DateTimeOffset end)
        {
            await foreach (var data in downloader.Download(start, end))
            {
                yield return parser.Parse(data);
            }   
        }
    }
}
