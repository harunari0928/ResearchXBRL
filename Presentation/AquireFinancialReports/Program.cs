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
            var maxParallelism = GetMaxParallelism(args);
            using var serviceProvider = CreateServiceProvider(maxParallelism);
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
            var fromArgKey = "--from";
            if (args.Contains(fromArgKey))
            {
                from = new DateTimeOffset(GetDateTimeArg(args, fromArgKey), TimeSpan.FromHours(9));
            }
            var toArgKey = "--to";
            if (args.Contains(toArgKey))
            {
                to = new DateTimeOffset(GetDateTimeArg(args, toArgKey), TimeSpan.FromHours(9));
            }
            return (from, to);
        }
        private static DateTime GetDateTimeArg(in string[] args, string argKey)
        {
            if (DateTime.TryParse(args.SkipWhile(x => x != argKey).Skip(1).FirstOrDefault(), out var datetime))
            {
                return datetime;
            }
            throw new ArgumentException($"{argKey}には、日付を指定してください");
        }
        private static int GetMaxParallelism(in string[] args)
        {
            var maxParallelismArgKey = "--maxParallelism";
            if (args.Contains(maxParallelismArgKey))
            {
                return GetIntArg(args, maxParallelismArgKey);
            }
            return 1; // デフォルトはシングルスレッド実行
        }
        private static int GetIntArg(in string[] args, string argKey)
        {
            if (int.TryParse(args.SkipWhile(x => x != argKey).Skip(1).FirstOrDefault(), out var intValue))
            {
                return intValue;
            }
            throw new ArgumentException($"{argKey}には、数値を指定してください");
        }
        private static ServiceProvider CreateServiceProvider(int maxParallelism)
        {
            return new ServiceCollection()
                .AddTransient<IAquireFinancialReportsUsecase>(x
                    => new AquireFinancialReportsInteractor(
                        x.GetService<IEdinetXBRLDownloader>(),
                        x.GetService<IEdinetXBRLParser>(),
                        x.GetService<IFinancialReportRepository>(),
                        x.GetService<IAquireFinancialReportsPresenter>(),
                        maxParallelism
                    ))
                .AddTransient<IEdinetXBRLDownloader>(x => new AllSecurityReportsDownloader(x.GetService<IHttpClientFactory>(), "v1"))
                .AddTransient<IEdinetXBRLParser, EdinetXBRLParser>()
                .AddTransient<IFinancialReportRepository, FinancialReportRepository>()
                .AddSingleton<IFileStorage>(_ => new LocalStorage(".tmp"))
                .AddSingleton<IAquireFinancialReportsPresenter, ConsolePresenter>()
                .AddHttpClient()
                .BuildServiceProvider();
        }
    }
}
