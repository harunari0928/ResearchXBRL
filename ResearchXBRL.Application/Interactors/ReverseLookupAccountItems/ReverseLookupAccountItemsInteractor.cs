using System.Collections.Generic;
using System.Threading.Tasks;
using ResearchXBRL.Application.DTO.ReverseLookupAccountItems;
using ResearchXBRL.Application.DTO.Results;
using ResearchXBRL.Application.QueryServices.ReverseLookupAccountItems;
using ResearchXBRL.Application.Usecase.ReverseLookupAccountItems;
using ResearchXBRL.Domain.ReverseLookupAccountItems.AccountItems;

namespace ResearchXBRL.Application.Interactors.ReverseLookupAccountItems;

public sealed class ReverseLookupAccountItemsInteractor : IReverseLookupAccountItemsUsecase
{
    private readonly IReverseDictionaryQueryService reverseDictionaryQueryService;
    private readonly IReverseLookupQueryService reverseLookupQueryService;
    private readonly IAccountItemsRepository repository;
    private readonly IReverseLookupAccountItemsPresenter presenter;

    public ReverseLookupAccountItemsInteractor(
        IReverseDictionaryQueryService reverseLookupTableQueryService,
        IReverseLookupQueryService reverseLookupQueryService,
        IAccountItemsRepository repository,
        IReverseLookupAccountItemsPresenter presenter)
    {
        this.reverseDictionaryQueryService = reverseLookupTableQueryService;
        this.reverseLookupQueryService = reverseLookupQueryService;
        this.repository = repository;
        this.presenter = presenter;
    }

    /// <inheritdoc/>
    public async ValueTask Handle()
    {
        switch (reverseDictionaryQueryService.Get())
        {
            case Success<IAsyncEnumerable<FinancialReport>> success:
                {
                    var normalizedAccountItems = GetNormalizedAccountItems(success.Value);

                    await repository.Add(normalizedAccountItems);
                    break;
                }

            case Abort<IAsyncEnumerable<FinancialReport>> abort:
                presenter.Warn(abort.Message);
                break;
        }
    }

    private async IAsyncEnumerable<AccountItem> GetNormalizedAccountItems(IAsyncEnumerable<FinancialReport> financialReports)
    {
        await foreach (var report in financialReports)
        {
            foreach (var lookupResult in await reverseLookupQueryService.Lookup(report))
            {
                yield return new AccountItem(lookupResult.NormalizedName, lookupResult.OriginalName);
            }
        }
    }
}
