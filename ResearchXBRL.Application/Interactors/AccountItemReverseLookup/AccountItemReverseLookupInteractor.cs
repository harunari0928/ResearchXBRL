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

    public async ValueTask Handle()
    {
        var reverseDictionary = reverseDictionaryQueryService.Get();

        var normalizedAccountItems = await GetNormalizedAccountItems(reverseDictionary).ToListAsync();

        await repository.Add(normalizedAccountItems);
    }

    private async IAsyncEnumerable<AccountItem> GetNormalizedAccountItems(IEnumerable<ReverseLookupParameters> reverseDictionary)
    {
        foreach (var item in reverseDictionary)
        {
            foreach (var result in await reverseLookupQueryService.Lookup(item))
            {
                yield return new AccountItem(result.NormalizedName, result.OriginalName);
            }
        }
    }
}
