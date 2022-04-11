﻿using Microsoft.Extensions.DependencyInjection;
using ResearchXBRL.Application.Interactors.ReverseLookupAccountItems;
using ResearchXBRL.Application.QueryServices.ReverseLookupAccountItems;
using ResearchXBRL.Application.Usecase.ReverseLookupAccountItems;
using ResearchXBRL.Domain.ReverseLookupAccountItems.AccountItems;
using ResearchXBRL.Infrastructure.ReverseLookupAccountItems.AccountItems;
using ResearchXBRL.Infrastructure.QueryServices.ReverseLookupAccountItems;
using ResearchXBRL.Infrastructure.Shared.Extensions;
using ResearchXBRL.Infrastructure.Shared.FileStorages;
using ReverseLookupAccountItems;

await ConsoleApp.RunAsync(args, async (
    [Option("f", "name of reverse dictionary file.")] string fileName,
    [Option("o", "name of output file.")] string ouputFileName) =>
{
    await using var serviceProvider = CreateServiceProvider(fileName, ouputFileName);
    var usecase = serviceProvider?
        .GetService<IReverseLookupAccountItemsUsecase>()
        ?? throw new System.Exception("usecaseモジュールのDIに失敗しました");
    await usecase.Handle();
});

static ServiceProvider CreateServiceProvider(string fileName, string outputFileName) =>
     new ServiceCollection()
        .AddTransient<IReverseLookupAccountItemsUsecase, ReverseLookupAccountItemsInteractor>()
        .AddTransient<IReverseLookupAccountItemsPresenter, ConsolePresenter>()
        .AddTransient<IReverseDictionaryQueryService>(x => new ReverseDictionaryCSVQueryService(x.GetService<IFileStorage>()!, fileName))
        .AddTransient<IReverseLookupQueryService, ReverseLookupQueryService>()
        .AddTransient<IAccountItemsRepository>(x => new AccountItemsRepository(x.GetService<IFileStorage>()!, outputFileName))
        .AddSFTPFileStorage()
        .BuildServiceProvider();