using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ResearchXBRL.Application.DTO.AccountItemReverseLookup;
using ResearchXBRL.Application.QueryServices.AccountItemReverseLookup;
using ResearchXBRL.Domain.AccountItemReverseLookup.AccountItems;

namespace ResearchXBRL.Application.Interactors.AccountItemReverseLookup;
public sealed class AccountItemReverseLookupInteractor
{
    private readonly IReverseDictionaryQueryService reverseDictionaryQueryService;
    private readonly IReverseLookupQueryService reverseLookupQueryService;
    private readonly IAccountItemRepository repository;

    public AccountItemReverseLookupInteractor(
        IReverseDictionaryQueryService reverseLookupTableQueryService,
        IReverseLookupQueryService reverseLookupQueryService,
        IAccountItemRepository repository)
    {
        this.reverseDictionaryQueryService = reverseLookupTableQueryService;
        this.reverseLookupQueryService = reverseLookupQueryService;
        this.repository = repository;
    }

    /// <summary>
    /// 財務諸表情報からXBRL会計項目名を逆引きし、リポジトリに保存する
    /// </summary>
    /// <returns></returns>
    public async ValueTask Handle()
    {
        var financialReports = reverseDictionaryQueryService.Get();

        var normalizedAccountItems = GetNormalizedAccountItems(financialReports);

        await repository.Add(normalizedAccountItems);
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
