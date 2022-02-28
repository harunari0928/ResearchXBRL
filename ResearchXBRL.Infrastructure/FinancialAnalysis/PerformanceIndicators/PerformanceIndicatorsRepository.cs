using System;
using System.Threading.Tasks;
using ResearchXBRL.Domain.FinancialAnalysis.PerformanceIndicators;

namespace ResearchXBRL.Infrastructure.FinancialAnalysis.PerformanceIndicators;

public class PerformanceIndicatorsRepository : IPerformanceIndicatorsRepository
{
    public ValueTask<Domain.FinancialAnalysis.PerformanceIndicators.PerformanceIndicators> Get(string corporationId)
    {
        throw new NotImplementedException();
    }
}
