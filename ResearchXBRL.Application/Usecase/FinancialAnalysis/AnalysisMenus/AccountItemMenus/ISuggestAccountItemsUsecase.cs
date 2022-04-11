using System.Collections.Generic;
using System.Threading.Tasks;
using ResearchXBRL.Application.ViewModel.FinancialAnalysis.AnalysisMenus.AccountItems;

namespace ResearchXBRL.Application.Usecase.FinancialAnalysis.AnalysisMenus.AccountItemMenus;

public interface ISuggestAccountItemsUsecase
{
    Task<IReadOnlyList<AccountItemViewModel>> Handle(string keyword);
}
