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
    public static async Task Main(string[] _)
    {
        using var serviceProvider = CreateServiceProvider();
        var usecase = serviceProvider?
            .GetService<IAccountItemReverseLookupUsecase>()
            ?? throw new System.Exception("usecaseモジュールのDIに失敗しました");
        await usecase.Handle();
    }

    private static ServiceProvider CreateServiceProvider()
    {
        return new ServiceCollection()
            .AddTransient<IAccountItemReverseLookupUsecase, AccountItemReverseLookupInteractor>()
            .AddTransient<IReverseDictionaryQueryService>(x => new ReverseDictionaryCSVQueryService(x.GetService<IFileStorage>(), "./Presentation/AccountItemReverseLookup/ReverseLookupDictionary.csv"))
            .AddTransient<IReverseLookupQueryService, ReverseLookupQueryService>()
            .AddTransient<IAccountItemsRepository, AccountItemsRepository>()
            .AddSingleton<IFileStorage>(_ => new LocalStorage("."))
            .AddHttpClient()
            .BuildServiceProvider();
    }
}