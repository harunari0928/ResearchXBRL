using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ResearchXBRL.Application.Interactors.ImportAccountItems;
using ResearchXBRL.Application.Services;
using ResearchXBRL.Application.Usecase.ImportAccountItems;
using ResearchXBRL.Domain.ImportAccountItems.AccountItems;
using ResearchXBRL.Infrastructure.ImportAccountItems.AccountItems;
using ResearchXBRL.Infrastructure.Shared.FileStorages;
using ResearchXBRL.Infrastructure.Services.TaxonomyDownloaders;
using ResearchXBRL.Infrastructure.Services.TaxonomyParsers;
using ResearchXBRL.Infrastructure.Shared.Extensions;
using Microsoft.Extensions.Logging;
using System;

await ConsoleApp.RunAsync(args, async () =>
{
    await using var serviceProvider = CreateServiceProvider();
    var logger = serviceProvider.GetService<ILogger>()
        ?? throw new Exception($"{nameof(ILogger)}モジュールのDIに失敗しました");
    try
    {
        var usecase = serviceProvider.GetService<IAquireAccoumtItemsUsecase>()
            ?? throw new Exception($"{nameof(IAquireAccoumtItemsUsecase)}モジュールのDIに失敗しました");
        await usecase.Handle();
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "ハンドリングされていないエラー");
        Environment.ExitCode = 1;
    }
});

static ServiceProvider CreateServiceProvider()
{
    return new ServiceCollection()
        .AddTransient<IAquireAccoumtItemsUsecase, AquireAccountItemsInteractor>()
        .AddTransient<ITaxonomyDownloader, TaxonomyDownloader>()
        .AddTransient<ITaxonomyParser, TaxonomyParser>()
        .AddTransient<IAccountItemsRepository, AccountItemsRepository>()
        .AddSingleton<IFileStorage>(_ => new LocalFileStorage("/.tmp"))
        .AddHttpClient()
        .AddNLog()
        .BuildServiceProvider();
}
