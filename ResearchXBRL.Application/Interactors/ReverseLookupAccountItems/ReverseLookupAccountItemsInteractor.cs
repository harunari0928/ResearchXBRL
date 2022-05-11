using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ResearchXBRL.Application.DTO.ReverseLookupAccountItems;
using ResearchXBRL.Application.DTO.Results;
using ResearchXBRL.Application.QueryServices.ReverseLookupAccountItems;
using ResearchXBRL.Application.Usecase.ReverseLookupAccountItems;
using ResearchXBRL.Domain.ReverseLookupAccountItems.AccountItems;
using Microsoft.Extensions.Logging;

namespace ResearchXBRL.Application.Interactors.ReverseLookupAccountItems;

public sealed class ReverseLookupAccountItemsInteractor : IReverseLookupAccountItemsUsecase, IDisposable, IAsyncDisposable
{
    private readonly IReverseDictionaryQueryService reverseDictionaryQueryService;
    private readonly IReverseLookupQueryService reverseLookupQueryService;
    private readonly IAccountItemsRepository repository;
    private readonly ILogger<ReverseLookupAccountItemsInteractor> logger;

    public ReverseLookupAccountItemsInteractor(
        IReverseDictionaryQueryService reverseLookupTableQueryService,
        IReverseLookupQueryService reverseLookupQueryService,
        IAccountItemsRepository repository,
        ILogger<ReverseLookupAccountItemsInteractor> logger)
    {
        this.reverseDictionaryQueryService = reverseLookupTableQueryService;
        this.reverseLookupQueryService = reverseLookupQueryService;
        this.repository = repository;
        this.logger = logger;
        logger.LogInformation("Start ReverseLookupAccountItems");
    }

    public void Dispose()
    {
        logger.LogInformation("End ReverseLookupAccountItems");
    }

    public async ValueTask DisposeAsync()
    {
        logger.LogInformation("End ReverseLookupAccountItems");
        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async ValueTask Handle()
    {
        switch (reverseDictionaryQueryService.Get())
        {
            case Succeeded<IAsyncEnumerable<FinancialReport>> success:
                {
                    var normalizedAccountItems = GetNormalizedAccountItems(success.Value);

                    await repository.Add(normalizedAccountItems);
                    break;
                }

            case Abort<IAsyncEnumerable<FinancialReport>> abort:
                logger.LogWarning(abort.Message);
                break;
        }
    }

    private async IAsyncEnumerable<AccountItem> GetNormalizedAccountItems(IAsyncEnumerable<FinancialReport> financialReports)
    {
        await foreach (var report in financialReports)
        {
            foreach (var lookupResult in await reverseLookupQueryService.Lookup(report))
            {
                yield return new AccountItem(
                    lookupResult.NormalizedName,
                     lookupResult.OriginalName,
                     lookupResult.SecuritiesCode,
                     lookupResult.FiscalYear);
            }
        }
    }
}
