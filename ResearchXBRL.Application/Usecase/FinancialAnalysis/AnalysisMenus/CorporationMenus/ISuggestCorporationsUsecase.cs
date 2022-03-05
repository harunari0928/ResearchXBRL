using System.Collections.Generic;
using System.Threading.Tasks;
using ResearchXBRL.Application.ViewModel.FinancialAnalysis.AnalysisMenus.CorporationMenus;

namespace ResearchXBRL.Application.Usecase.FinancialAnalysis.AnalysisMenus.CorporationMenus;

public interface ISuggestCorporationsUsecase
{
    Task<IReadOnlyList<CorporationViewModel>> Handle(string keyword);
}
