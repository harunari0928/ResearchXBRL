using System.Threading.Tasks;
using ResearchXBRL.Application.DTO.FinancialAnalysis.PerformanceIndicators;

namespace ResearchXBRL.Application.QueryServices.FinancialAnalysis.PerformanceIndicators;

public interface IPerformanceIndicatorQueryService
{
    ValueTask<PerformanceIndicator> Get(string corporationId);
}
