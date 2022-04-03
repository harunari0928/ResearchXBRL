using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ResearchXBRL.Application.Interactors.AccountItemReverseLookup;
using ResearchXBRL.Application.QueryServices.AccountItemReverseLookup;
using ResearchXBRL.Application.Usecase.AccountItemReverseLookup;
using ResearchXBRL.Domain.AccountItemReverseLookup.AccountItems;
using ResearchXBRL.Infrastructure.AccountItemReverseLookup.AccountItems;
using ResearchXBRL.Infrastructure.QueryServices.AccountItemReverseLookup;
using ResearchXBRL.Infrastructure.Services;
using ResearchXBRL.Infrastructure.Services.FileStorages;

namespace AccountItemReverseLookup;

class Program
{
    public static async Task Main(string[] args)
    {
        await ConsoleApp.RunAsync(args, async ([Option("f", "name of reverse dictionary file.")] string fileName) =>
        {
            using var serviceProvider = CreateServiceProvider(fileName);
            var usecase = serviceProvider?
                .GetService<IAccountItemReverseLookupUsecase>()
                ?? throw new System.Exception("usecaseモジュールのDIに失敗しました");
            await usecase.Handle();
        });
    }

    private static ServiceProvider CreateServiceProvider(string fileName)
    {
        return new ServiceCollection()
            .AddTransient<IAccountItemReverseLookupUsecase, AccountItemReverseLookupInteractor>()
            .AddTransient<IAccountItemReverseLookupPresenter, ConsolePresenter>()
            .AddTransient<IReverseDictionaryQueryService>(x => new ReverseDictionaryCSVQueryService(x.GetService<IFileStorage>()!, fileName))
            .AddTransient<IReverseLookupQueryService, ReverseLookupQueryService>()
            .AddTransient<IAccountItemsRepository, AccountItemsRepository>()
            .AddSingleton<IFileStorage, SFTPFileStorage>()
            .BuildServiceProvider();
    }
}