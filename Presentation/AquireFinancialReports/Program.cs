using System;
using System.Net.Http;
using AquireFinancialReports.Presenter;
using Microsoft.Extensions.DependencyInjection;
using ResearchXBRL.Application.DTO.Results;
using ResearchXBRL.Application.ImportFinancialReports;
using ResearchXBRL.Application.Services;
using ResearchXBRL.Application.Usecase.ImportFinancialReports;
using ResearchXBRL.Domain.ImportFinancialReports.FinancialReports;
using ResearchXBRL.Infrastructure.ImportFinancialReports.FinancialReports;
using ResearchXBRL.Infrastructure.Services.EdinetXBRLDownloaders;
using ResearchXBRL.Infrastructure.Services.EdinetXBRLParser;
using ResearchXBRL.Infrastructure.Shared.FileStorages;

await ConsoleApp.RunAsync(args, async (
    [Option("f", "get date from.")] string? from,
    [Option("t", "get date to.")] string? to,
    [Option("p", "max parallelism.")] int? maxParallelism) =>
{
    // デフォルトはシングルスレッド実行
    await using var serviceProvider = CreateServiceProvider(maxParallelism ?? 1);
    var usecase = serviceProvider?
        .GetService<IAquireFinancialReportsUsecase>()
        ?? throw new System.Exception("usecaseモジュールのDIに失敗しました");
    await usecase.Handle(GetAquireFromTo(from, to));
});

static IResult<(DateTimeOffset, DateTimeOffset)> GetAquireFromTo(in string? from, in string? to)
{
    return (ConvertToDateTimeOffset(from), ConvertToDateTimeOffset(to)) switch
    {
        (Succeeded<DateTimeOffset?> mayBeFromDateTime, Succeeded<DateTimeOffset?> mayBeToDateTime) =>
            new Succeeded<(DateTimeOffset, DateTimeOffset)>((
                // デフォルトでは直近1日の報告書を取得する
                mayBeFromDateTime.Value ?? DateTimeOffset.Now.AddDays(-1),
                mayBeToDateTime.Value ?? DateTimeOffset.Now)),
        (Failed<DateTimeOffset?>, _) =>
            new Failed<(DateTimeOffset, DateTimeOffset)>
            {
                Message = $"{nameof(from)}には日付を指定してください。"
            },
        (_, Failed<DateTimeOffset?>) =>
             new Failed<(DateTimeOffset, DateTimeOffset)>
             {
                 Message = $"{nameof(to)}には日付を指定してください。"
             },
        _ => throw new NotSupportedException($"{nameof(ConvertToDateTimeOffset)}メソッドから予期しない戻り値の型が返されました。返された型に対する処理の実装をお願いします。")
    };
}

static ServiceProvider CreateServiceProvider(int maxParallelism)
{
    return new ServiceCollection()
        .AddTransient<IAquireFinancialReportsUsecase>(x
            => new AquireFinancialReportsInteractor(
                x.GetService<IEdinetXBRLDownloader>() ?? throw new Exception($"{nameof(IEdinetXBRLDownloader)}のDIに失敗しました"),
                x.GetService<IEdinetXBRLParser>() ?? throw new Exception($"{nameof(IEdinetXBRLParser)}のDIに失敗しました"),
                x.GetService<IFinancialReportsRepository>() ?? throw new Exception($"{nameof(IFinancialReportsRepository)}のDIに失敗しました"),
                x.GetService<IAquireFinancialReportsPresenter>() ?? throw new Exception($"{nameof(IAquireFinancialReportsPresenter)}のDIに失敗しました"),
                maxParallelism
            ))
        .AddTransient<IEdinetXBRLDownloader>(x =>
            new SecurityReportsDownloader(
                x.GetService<IHttpClientFactory>() ?? throw new Exception($"{nameof(IHttpClientFactory)}のDIに失敗しました"),
         "v1"))
        .AddSingleton<IAquireFinancialReportsPresenter, ConsolePresenter>()
        .AddTransient<IEdinetXBRLParser, EdinetXBRLParser>()
        .AddTransient<IFinancialReportsRepository, FinancialReportsRepository>()
        .AddSingleton<IFileStorage>(_ => new LocalFileStorage(".tmp"))
        .AddHttpClient()
        .BuildServiceProvider();
}

static IResult<DateTimeOffset?> ConvertToDateTimeOffset(in string? datetimeStr)
{
    if (datetimeStr is null)
    {
        return new Succeeded<DateTimeOffset?>(null);
    }
    if (DateTime.TryParse(datetimeStr, out var datetime))
    {
        return new Succeeded<DateTimeOffset?>(
            new DateTimeOffset(datetime, TimeSpan.FromHours(9))
        );
    }

    return new Failed<DateTimeOffset?>();
}