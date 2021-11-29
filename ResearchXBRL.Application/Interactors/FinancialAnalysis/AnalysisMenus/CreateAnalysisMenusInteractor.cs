using System.Linq;
using System;
using System.Threading.Tasks;
using ResearchXBRL.Application.DTO;
using ResearchXBRL.Application.Usecase.FinancialAnalysis.AnalysisMenus;
using ResearchXBRL.Domain.FinancialAnalysis.AnalysisMenus;

namespace ResearchXBRL.Application.Interactors.FinancialAnalysis.AnalysisMenus
{
    public class CreateAnalysisMenusInteractor : ICreateAnalysisMenusUsecase
    {
        private readonly IAnalysisMenuRepository repository;

        public CreateAnalysisMenusInteractor(IAnalysisMenuRepository repository)
        {
            this.repository = repository;
        }

        public async Task<AnalysisMenuViewModel> Handle()
        {
            var analysisMenu = await repository.Get();
            return new AnalysisMenuViewModel
            {
                AccountItems = analysisMenu.AccountItems.Select(x => x.Name).ToArray(),
                Corporations = analysisMenu.Corporations.Select(x => x.Name).ToArray()
            };
        }
    }
}
