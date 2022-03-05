using System.Threading.Tasks;
using ResearchXBRL.Application.ViewModel.FinancialAnalysis.PerformanceIndicators;

namespace ResearchXBRL.Application.Usecase.FinancialAnalysis.PerformanceIndicators;

public interface IGetPerformanceIndicatorsUsecase
{
    ValueTask<PerformanceIndicatorViewModel> Handle(string corporationId);
}
