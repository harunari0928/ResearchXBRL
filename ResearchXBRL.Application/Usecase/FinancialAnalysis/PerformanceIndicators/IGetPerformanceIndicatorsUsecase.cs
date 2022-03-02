using System.Threading.Tasks;
using ResearchXBRL.Application.DTO.FinancialAnalysis.PerformanceIndicators;

namespace ResearchXBRL.Application.Usecase.FinancialAnalysis.PerformanceIndicators;

public interface IGetPerformanceIndicatorsUsecase
{
    ValueTask<PerformanceIndicatorsViewModel> Handle(string corporationId);
}
