using System.Collections.Generic;
using System.Threading.Tasks;
using ResearchXBRL.Application.DTO.AccountItemReverseLookup;
using ResearchXBRL.Application.DTO.Results;
using ResearchXBRL.Application.QueryServices.AccountItemReverseLookup;
using ResearchXBRL.Application.Usecase.AccountItemReverseLookup;
using ResearchXBRL.Domain.AccountItemReverseLookup.AccountItems;

namespace ResearchXBRL.Application.Interactors.AccountItemReverseLookup;

public sealed class AccountItemReverseLookupInteractor : IAccountItemReverseLookupUsecase
{
    private readonly IReverseDictionaryQueryService reverseDictionaryQueryService;
    private readonly IReverseLookupQueryService reverseLookupQueryService;
    private readonly IAccountItemsRepository repository;
    private readonly IAccountItemReverseLookupPresenter presenter;

    public AccountItemReverseLookupInteractor(
        IReverseDictionaryQueryService reverseLookupTableQueryService,
        IReverseLookupQueryService reverseLookupQueryService,
        IAccountItemsRepository repository,
        IAccountItemReverseLookupPresenter presenter)
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
