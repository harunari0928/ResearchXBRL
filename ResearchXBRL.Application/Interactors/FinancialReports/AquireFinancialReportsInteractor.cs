using System.Collections.Concurrent;
using System.Threading;
using ResearchXBRL.Application.Services;
using ResearchXBRL.Application.Usecase.FinancialReports;
using ResearchXBRL.Domain.FinancialReports;
using System;
using System.Threading.Tasks;
using System.Linq;
using ResearchXBRL.Application.DTO;

namespace ResearchXBRL.Application.FinancialReports
{
    public sealed class AquireFinancialReportsInteractor : IAquireFinancialReportsUsecase, IDisposable
    {
        private readonly IEdinetXBRLDownloader downloader;
        private readonly IEdinetXBRLParser parser;
        private readonly IFinancialReportRepository reportRepository;
        private readonly IAquireFinancialReportsPresenter presenter;
        private readonly SemaphoreSlim semaphore;
        private readonly ConcurrentStack<Task> jobs = new();
        private readonly ConcurrentStack<Exception> exceptions = new();

        public AquireFinancialReportsInteractor(
            in IEdinetXBRLDownloader downloader,
            in IEdinetXBRLParser parser,
            in IFinancialReportRepository reportRepository,
            in IAquireFinancialReportsPresenter presenter,
            in int maxParallelism)
        {
            this.downloader = downloader;
            this.parser = parser;
            this.reportRepository = reportRepository;
            this.presenter = presenter;
            semaphore = new(maxParallelism);
        }

        public void Dispose()
        {
            semaphore.Dispose();
        }

        public async Task Handle(DateTimeOffset start, DateTimeOffset end)
        {
            if (start > end)
            {
                throw new ArgumentException($"{nameof(start)}よりも{nameof(end)}を後の日付にしてください");
            }

            await foreach (var data in downloader.Download(start, end))
            {
                await semaphore.WaitAsync();
                jobs.Push(SaveReport(start, end, data));
            }
            await Task.WhenAll(jobs);

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }

            presenter.Complete();
        }

        private async Task SaveReport(DateTimeOffset start, DateTimeOffset end, EdinetXBRLData data)
        {
            presenter.Progress(start.DateTime, end.DateTime, data.DocumentDateTime);
            try
            {
                if (await reportRepository.IsExists(data.DocumentId))
                {
                    return;
                }

                var report = await parser.Parse(data);
                await reportRepository.Write(report);
            }
            catch (Exception ex)
            {
                presenter.Error($@"インポート中にエラーが発生しました
document_id: {data.DocumentId}
document_date_time: {data.DocumentDateTime}
", ex);
                exceptions.Push(ex);
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
