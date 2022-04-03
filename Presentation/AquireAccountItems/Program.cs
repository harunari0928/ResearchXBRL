using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ResearchXBRL.Application.Interactors.ImportAccountItems;
using ResearchXBRL.Application.Services;
using ResearchXBRL.Application.Usecase.ImportAccountItems;
using ResearchXBRL.Domain.ImportAccountItems.AccountItems;
using ResearchXBRL.Infrastructure.ImportAccountItems.AccountItems;
using ResearchXBRL.Infrastructure.Services;
using ResearchXBRL.Infrastructure.Services.FileStorages;
using ResearchXBRL.Infrastructure.Services.TaxonomyDownloaders;
using ResearchXBRL.Infrastructure.Services.TaxonomyParsers;

namespace AquireAccountItems;

class Program
{
    static async Task Main(string[] _)
    {
        using var serviceProvider = CreateServiceProvider();
        var usecase = serviceProvider
            .GetService<IAquireAccoumtItemsUsecase>();
        await usecase.Handle();
    }

    private static ServiceProvider CreateServiceProvider()
    {
        return new ServiceCollection()
            .AddTransient<IAquireAccoumtItemsUsecase, AquireAccountItemsInteractor>()
            .AddTransient<ITaxonomyDownloader, TaxonomyDownloader>()
            .AddTransient<ITaxonomyParser, TaxonomyParser>()
            .AddTransient<IAccountItemsRepository, AccountItemsRepository>()
            .AddSingleton<IFileStorage>(_ => new LocalFileStorage("/.tmp"))
            .AddHttpClient()
            .BuildServiceProvider();
    }
}
