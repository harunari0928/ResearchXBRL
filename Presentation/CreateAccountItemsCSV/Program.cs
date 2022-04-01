using Microsoft.Extensions.DependencyInjection;
using ResearchXBRL.Application.Interactors.ImportAccountItems.Transfer;
using ResearchXBRL.Application.Services;
using ResearchXBRL.Application.Usecase.ImportAccountItems.Transfer;
using ResearchXBRL.Domain.ImportAccountItems.AccountItems;
using ResearchXBRL.Infrastructure.ImportAccountItems.AccountItems;
using ResearchXBRL.Infrastructure.Services.TaxonomyParsers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ResearchXBRL.Presentaion.CreateAccountItemsCSV
{
    class Program
    {
        static async Task Main(string[] _)
        {
            Console.WriteLine("accountSchemaFilePath(ex: ./*_2019-11-01.xsd) >");
            var accountSchemaFilePath = Console.ReadLine() ?? throw new Exception("入力してください");
            Console.WriteLine("accountItemLabelPath(ex: ./*_lab.xml) >");
            var accountItemLabelPath = Console.ReadLine() ?? throw new Exception("入力してください");
            Console.WriteLine("outputFilePath >");
            var outputFilePath = Console.ReadLine() ?? throw new Exception("入力してください");

            using var accountSchemaReader = new StreamReader(accountSchemaFilePath);
            using var accountItemLabelReader = new StreamReader(accountItemLabelPath);
            using var accountElementWriter = new StreamWriter(outputFilePath);
            using var serviceProvider = CreateServiceProvider(accountElementWriter);
            var usecase = serviceProvider
                .GetService<ITransferAccountItemsUsecase>()
                ?? throw new Exception("実行失敗");
            await usecase.Hundle(accountItemLabelReader.BaseStream, accountSchemaReader.BaseStream);
        }

        private static ServiceProvider CreateServiceProvider(TextWriter writer)
        {
            return new ServiceCollection()
                .AddTransient<ITaxonomyParser, TaxonomyParser>()
                .AddTransient<IAccountItemsWriter>(_ =>
                    new AccountItemsCSVWriter(writer))
                .AddTransient<ITransferAccountItemsPresenter, ConsolePresenter>()
                .AddSingleton<ITransferAccountItemsUsecase, TransferAccountItemsInteractor>()
                .BuildServiceProvider();
        }
    }
}
