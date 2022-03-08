using System.Threading.Tasks;

namespace ResearchXBRL.Application.QueryServices.FinancialAnalysis.PerformanceIndicators;

public interface ICorporationsQueryService
{
    ValueTask<bool> Exists(string corporationId);
}
