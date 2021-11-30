using System.Collections.Generic;
using System.Threading.Tasks;
using ResearchXBRL.Application.DTO.FinancialAnalysis.AnalysisMenus.AccountItemMenus;

namespace ResearchXBRL.Application.Usecase.FinancialAnalysis.AnalysisMenus.AccountItemMenus
{
    public interface ISuggestAccountItemsUsecase
    {
        Task<IReadOnlyList<AccountItemViewModel>> Handle(string keyword);
    }
}
