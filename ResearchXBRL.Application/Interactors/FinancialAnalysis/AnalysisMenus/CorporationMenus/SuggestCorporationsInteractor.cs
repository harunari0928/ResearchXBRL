using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ResearchXBRL.Application.DTO.FinancialAnalysis.AnalysisMenus.CorporationMenus;
using ResearchXBRL.Application.Usecase.FinancialAnalysis.AnalysisMenus.CorporationMenus;
using ResearchXBRL.Domain.FinancialAnalysis.AnalysisMenus.CorporationMenus;

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
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Enumerable.Empty<CorporationViewModel>().ToArray();
            }

            var corporationsMenu = await repository.GetProposals(keyword);
            return corporationsMenu.Corporations
                .Select(x => new CorporationViewModel
                {
                    Name = x.Name,
                    CorporationId = x.CorporationId
                }).ToArray();
        }
    }
}
