using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ResearchXBRL.Application.DTO;
using ResearchXBRL.Application.DTO.Results;
using ResearchXBRL.Application.Services;
using ResearchXBRL.Application.Usecase.ImportFinancialReports;
using ResearchXBRL.Domain.ImportFinancialReports.FinancialReports;

namespace ResearchXBRL.Application.ImportFinancialReports;

public sealed class AquireFinancialReportsInteractor : IAquireFinancialReportsUsecase, IDisposable
{
    private readonly IEdinetXBRLDownloader downloader;
    private readonly IEdinetXBRLParser parser;
    private readonly IFinancialReportsRepository reportRepository;
    private readonly IAquireFinancialReportsPresenter presenter;
    private readonly SemaphoreSlim semaphore;
    private readonly ConcurrentStack<Task> jobs = new();
    private readonly ConcurrentStack<Exception> exceptions = new();

    public AquireFinancialReportsInteractor(
        in IEdinetXBRLDownloader downloader,
        in IEdinetXBRLParser parser,
        in IFinancialReportsRepository reportRepository,
        in IAquireFinancialReportsPresenter presenter,
        in int maxParallelism)
    {
        presenter.Start();
        this.downloader = downloader;
        this.parser = parser;
        this.reportRepository = reportRepository;
        this.presenter = presenter;
        semaphore = new(maxParallelism);
    }

    public void Dispose()
    {
        presenter.Complete();
        semaphore.Dispose();
    }

    public async Task Handle(IResult<(DateTimeOffset, DateTimeOffset)> mayBeFromTo)
    {
        switch (await GetResult(mayBeFromTo))
        {
            case Succeeded:
                return;
            case Abort abort:
                presenter.Warn(abort.Message);
                return;
            case Failed failed:
                presenter.Error(failed.Message);
                return;
            default:
                throw new NotSupportedException($"{nameof(GetResult)}メソッドから予期しない型が検出されました。当該型に対する処理の実装をお願いします。");
        }
    }

    private async Task<IResult> GetResult(IResult<(DateTimeOffset, DateTimeOffset)> mayBeFromTo)
    {
        if (mayBeFromTo is not Succeeded<(DateTimeOffset, DateTimeOffset)> succeeded)
        {
            if (mayBeFromTo is Failed<(DateTimeOffset, DateTimeOffset)> failed)
            {
                return new Abort
                {
                    Message = failed.Message
                };
            }
            throw new NotSupportedException($"引数から予期しない型が検出されました。当該型に対する処理の実装をお願いします。");
        }

        var (start, end) = succeeded.Value;

        if (start > end)
        {
            return new Abort
            {
                Message = $"{nameof(start)}よりも{nameof(end)}を後の日付にしてください"
            };
        }

        await foreach (var data in downloader.Download(start, end))
        {
            await semaphore.WaitAsync();
            if (data is Abort<EdinetXBRLData> abort)
            {
                semaphore.Release();
                return new Abort { Message = abort.Message };
            }
            HandleDownloadResult(start, end, data);
        }
        await Task.WhenAll(jobs);

        if (exceptions.Any())
        {
            return new Failed
            {
                Message = $"{exceptions.Count}件のエラーが発生しました"
            };
        }

        return new Succeeded();
    }

    private void HandleDownloadResult(DateTimeOffset start, DateTimeOffset end, IResult<EdinetXBRLData> result)
    {
        switch (result)
        {
            case Succeeded<EdinetXBRLData> succeeded:
                jobs.Push(SaveReport(start, end, succeeded.Value));
                break;
            case Failed<EdinetXBRLData> failed:
                semaphore.Release();
                presenter.Error(failed.Message);
                exceptions.Push(new Exception(failed.Message));
                break;
            default:
                throw new NotSupportedException($"{nameof(GetResult)}メソッドから予期しない型が検出されました。当該型に対する処理の実装をお願いします。");
        }
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