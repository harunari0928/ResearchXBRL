using System.Threading.Tasks;
using ResearchXBRL.Application.DTO.FinancialAnalysis.PerformanceIndicators;

namespace ResearchXBRL.Application.QueryServices.FinancialAnalysis.PerformanceIndicators;

public interface IPerformanceIndicatorsQueryService
{
    ValueTask<PerformanceIndicator> Get(string corporationId);
}
