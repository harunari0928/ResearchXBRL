using Microsoft.Extensions.DependencyInjection;
using ResearchXBRL.Application.FinancialReports;
using ResearchXBRL.Application.Services;
using ResearchXBRL.Application.Usecase.FinancialReports;
using ResearchXBRL.Domain.FinancialReports;
using ResearchXBRL.Infrastructure.FinancialReports;
using ResearchXBRL.Infrastructure.Services;
using ResearchXBRL.Infrastructure.Services.EdinetXBRLDownloaders;
using ResearchXBRL.Infrastructure.Services.EdinetXBRLParser;
using ResearchXBRL.Infrastructure.Services.FileStorages;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AquireFinancialReports
{
    class Program
    {
        static async Task Main(string[] _)
        {
            using var serviceProvider = CreateServiceProvider();
            var usecase = serviceProvider
                .GetService<IAquireFinancialReporsUsecase>();
            await usecase.Handle(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now);
        }

        private static ServiceProvider CreateServiceProvider()
        {
            return new ServiceCollection()
                .AddTransient<IAquireFinancialReporsUsecase, AquireFinancialReportsInteractor>()
                .AddTransient<IEdinetXBRLDownloader>(x => new SecuritiesReportDownloader(x.GetService<IHttpClientFactory>(), "v1"))
                .AddTransient<IEdinetXBRLParser, EdinetXBRLParser>()
                .AddTransient<IFinancialReportRepository, FinancialReportRepository>()
                .AddSingleton<IFileStorage>(_ => new LocalStorage(".tmp"))
                .AddHttpClient()
                .BuildServiceProvider();
        }
    }
}
