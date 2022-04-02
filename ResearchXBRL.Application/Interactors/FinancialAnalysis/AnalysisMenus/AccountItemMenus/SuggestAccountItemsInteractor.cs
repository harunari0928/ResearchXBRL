using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using ResearchXBRL.Application.ViewModel.FinancialAnalysis.AnalysisMenus.AccountItemMenus;
using ResearchXBRL.Application.Usecase.FinancialAnalysis.AnalysisMenus.AccountItemMenus;
using ResearchXBRL.Domain.FinancialAnalysis.AnalysisMenus.AccountItems;

namespace ResearchXBRL.Application.Interactors.FinancialAnalysis.AnalysisMenus.AccountItemMenus
{
    public sealed class SuggestAccountItemsInteractor : ISuggestAccountItemsUsecase
    {
        private readonly IAccountItemsMenuRepository repository;

        public SuggestAccountItemsInteractor(
            IAccountItemsMenuRepository repository)
        {
            this.repository = repository;
        }

        public async Task<IReadOnlyList<AccountItemViewModel>> Handle(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Enumerable.Empty<AccountItemViewModel>().ToArray();
            }

            var accountItemMenu = await repository.GetProposals(keyword);
            return accountItemMenu.SuggestedAccountItems
                .Prepend(accountItemMenu.SearchedAccountItem)
                .OfType<AccountItem>()
                .Select(x => new AccountItemViewModel { Name = x.Name })
                .ToArray();
        }
    }
}
