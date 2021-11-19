using ResearchXBRL.Application.Services;
using ResearchXBRL.Application.Usecase.FinancialReports;
using ResearchXBRL.Domain.FinancialReports;
using System;
using System.Threading.Tasks;

namespace ResearchXBRL.Application.FinancialReports
{
    public sealed class AquireFinancialReportsInteractor : IAquireFinancialReporsUsecase
    {
        private readonly IEdinetXBRLDownloader downloader;
        private readonly IEdinetXBRLParser parser;
        private readonly IFinancialReportRepository reportRepository;

        public AquireFinancialReportsInteractor(
            IEdinetXBRLDownloader downloader,
            IEdinetXBRLParser parser,
            IFinancialReportRepository reportRepository)
        {
            this.downloader = downloader;
            this.parser = parser;
            this.reportRepository = reportRepository;
        }

        public async Task Handle(DateTimeOffset start, DateTimeOffset end)
        {
            if (start > end)
            {
                throw new ArgumentException($"{nameof(start)}よりも{nameof(end)}を後の日付にしてください");
            }

            await foreach (var data in downloader.Download(start, end))
            {
                var report = await parser.Parse(data);
                await reportRepository.Write(report);
            }   
        }
    }
}
