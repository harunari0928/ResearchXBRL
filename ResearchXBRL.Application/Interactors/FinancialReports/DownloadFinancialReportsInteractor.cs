using ResearchXBRL.Application.Services;
using ResearchXBRL.Application.Usecase.FinancialReports;
using System;
using System.Threading.Tasks;

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

        public async Task Handle(DateTimeOffset start, DateTimeOffset end)
        {
            if (start > end)
            {
                throw new ArgumentException($"{nameof(start)}よりも{nameof(end)}を後の日付にしてください");
            }

            await foreach (var data in downloader.Download(start, end))
            {
                await parser.Parse(data);
            }   
        }
    }
}
