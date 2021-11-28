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
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AquireFinancialReports
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var serviceProvider = CreateServiceProvider();
            var usecase = serviceProvider
                .GetService<IAquireFinancialReportsUsecase>();

            var (from, to) = GetAquireSpan(args);

            await usecase.Handle(from, to);
        }

        private static (DateTimeOffset, DateTimeOffset) GetAquireSpan(in string[] args)
        {
            // デフォルトでは直近1日の報告書を取得する
            var from = DateTimeOffset.Now.AddDays(-1);
            var to = DateTimeOffset.Now;
            var fromArg = "--from";
            if (args.Contains(fromArg))
            {
                from = new DateTimeOffset(GetDateTimeArg(args, fromArg), TimeSpan.FromHours(9));
            }
            var toArg = "--to";
            if (args.Contains(toArg))
            {
                to = new DateTimeOffset(GetDateTimeArg(args, toArg), TimeSpan.FromHours(9));
            }
            return (from, to);
        }

        private static DateTime GetDateTimeArg(in string[] args, string argKey)
        {
            if (DateTime.TryParse(args.SkipWhile(x => x == argKey).FirstOrDefault(), out var from))
            {
                return from;
            }
            throw new ArgumentException($"{nameof(argKey)}には、日付を指定してください");
        }

        private static ServiceProvider CreateServiceProvider()
        {
            return new ServiceCollection()
                .AddTransient<IAquireFinancialReportsUsecase, AquireFinancialReportsInteractor>()
                .AddTransient<IEdinetXBRLDownloader>(x => new SecuritiesReportDownloader(x.GetService<IHttpClientFactory>(), "v1"))
                .AddTransient<IEdinetXBRLParser, EdinetXBRLParser>()
                .AddTransient<IFinancialReportRepository, FinancialReportRepository>()
                .AddSingleton<IFileStorage>(_ => new LocalStorage(".tmp"))
                .AddSingleton<IAquireFinancialReportsPresenter, ConsolePresenter>()
                .AddHttpClient()
                .BuildServiceProvider();
        }
    }
}
