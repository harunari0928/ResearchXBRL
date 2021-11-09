using Microsoft.Extensions.DependencyInjection;
using ResearchXBRL.Application.AccountElements;
using ResearchXBRL.Application.Usecase.AccountElements.Transfer;
using ResearchXBRL.Domain.AccountElements;
using ResearchXBRL.Infrastructure.AccountElements;
using System;
using System.Threading.Tasks;

namespace ResearchXBRL.CreateAccountItemsCSV
{
    class Program
    {
        static async Task Main(string[] _)
        {
            Console.WriteLine("accountItemFilePath(ex: ./jppfs_cor_2019-11-01.xsd) >");
            var accountItemFilePath = Console.ReadLine() ?? throw new Exception("入力してください");
            Console.WriteLine("accountItemLabelPath(ex: ./jppfs_2019-11-01_lab.xml) >");
            var accountItemLabelPath = Console.ReadLine() ?? throw new Exception("入力してください");
            Console.WriteLine("outputFilePath >");
            var outputFilePath = Console.ReadLine() ?? throw new Exception("入力してください");

            using var serviceProvider = CreateServiceProvider(accountItemFilePath, accountItemLabelPath, outputFilePath);
            var useCase = serviceProvider
                .GetService<ITransferAccountElementsUsecase>()
                ?? throw new Exception("実行失敗");
            await useCase.Hundle();
        }

        private static ServiceProvider CreateServiceProvider(string accountItemFilePath, string accountItemLabelPath, string outputFilePath)
        {
            return new ServiceCollection()
                .AddTransient<IAccountElementReader>(_ =>
                    new AccountElementXMLReader(accountItemFilePath, accountItemLabelPath))
                .AddTransient<IAccountElementWriter>(_ =>
                    new AccountElementCSVWriter(outputFilePath))
                .AddTransient<ITransferAccountElementsPresenter, ConsolePresenter>()
                .AddSingleton<ITransferAccountElementsUsecase, TransferAccountElementsInteractor>()
                .BuildServiceProvider();
        }
    }
}
