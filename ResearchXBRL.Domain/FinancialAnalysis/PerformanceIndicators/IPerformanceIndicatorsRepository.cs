using System.Threading.Tasks;

namespace ResearchXBRL.Domain.FinancialAnalysis.PerformanceIndicators
{
    public interface IPerformanceIndicatorsRepository
    {
        ValueTask<PerformanceIndicators> Get(string corporationId);
    }
}
