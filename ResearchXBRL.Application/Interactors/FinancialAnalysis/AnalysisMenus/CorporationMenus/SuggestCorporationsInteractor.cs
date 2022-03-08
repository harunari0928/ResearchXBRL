using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ResearchXBRL.Application.ViewModel.FinancialAnalysis.AnalysisMenus.CorporationMenus;
using ResearchXBRL.Application.Usecase.FinancialAnalysis.AnalysisMenus.CorporationMenus;
using ResearchXBRL.Domain.FinancialAnalysis.AnalysisMenus.Corporations;

namespace ResearchXBRL.Application.Interactors.FinancialAnalysis.AnalysisMenus.CorporationMenus
{
    public sealed class SuggestCorporationsInteractor : ISuggestCorporationsUsecase
    {
        private readonly ICorporationMenuRepository repository;

        public SuggestCorporationsInteractor(
            ICorporationMenuRepository repository)
        {
            this.repository = repository;
        }

        public async Task<IReadOnlyList<CorporationViewModel>> Handle(string keyword)
        {
            var modifiedKeyword = CleansingKeyword(keyword);

            if (string.IsNullOrWhiteSpace(modifiedKeyword))
            {
                return Enumerable.Empty<CorporationViewModel>().ToArray();
            }

            var corporationsMenu = await repository.GetProposals(modifiedKeyword);
            return corporationsMenu.Corporations
                .Select(x => new CorporationViewModel
                {
                    Name = x.Name,
                    CorporationId = x.CorporationId
                }).ToArray();
        }

        private static string CleansingKeyword(string keyword)
        {
            // ほぼ全企業、"株式"や"会社"というワードが入っているのでこれを無視
            return keyword
                    .Replace("株式", "")
                    .Replace("会社", "")
                    .Trim();
        }
    }
}
