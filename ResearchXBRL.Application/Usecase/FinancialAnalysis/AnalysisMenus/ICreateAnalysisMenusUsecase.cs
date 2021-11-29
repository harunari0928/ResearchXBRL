using System.Threading.Tasks;
using ResearchXBRL.Application.DTO;

namespace ResearchXBRL.Application.Usecase.FinancialAnalysis.AnalysisMenus
{
    public interface ICreateAnalysisMenusUsecase
    {
        Task<AnalysisMenuViewModel> Handle();
    }
}
