using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ResearchXBRL.Application.Services;
using ResearchXBRL.Infrastructure.Services.TaxonomyDownloaders;
using ResearchXBRL.Infrastructure.Services.TaxonomyParsers;
using ResearchXBRL.Infrastructure.Services;
using ResearchXBRL.Infrastructure.Services.FileStorages;
using ResearchXBRL.Domain.AccountElements;
using ResearchXBRL.Infrastructure.AccountElements;
using ResearchXBRL.Application.Usecase.AccountElements;
using ResearchXBRL.Application.Interactors.AccountElements;

namespace AquireAccountItems
{
    class Program
    {
        static async Task Main(string[] _)
        {
            var usecase = CreateServiceProvider()
                .GetService<IAquireAccoumtElementsUsecase>();
            await usecase.Handle();
        }

        private static ServiceProvider CreateServiceProvider()
        {
            return new ServiceCollection()
                .AddTransient<IAquireAccoumtElementsUsecase, AquireAccoumtElementsInteractor>()
                .AddTransient<ITaxonomyDownloader, TaxonomyDownloader>()
                .AddTransient<ITaxonomyParser, TaxonomyParser>()
                .AddTransient<IAccountElementRepository, AccountElementRepository>()
                .AddSingleton<IFileStorage>(_ => new LocalStorage(".tmp"))
                .AddHttpClient()
                .BuildServiceProvider();
        }
    }
}
