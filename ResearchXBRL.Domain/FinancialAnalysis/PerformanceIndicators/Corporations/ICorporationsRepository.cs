using System;
using System.Threading.Tasks;

namespace ResearchXBRL.Domain.FinancialAnalysis.PerformanceIndicators.Corporations
{
    public interface ICorporationsRepository
    {
        ValueTask<bool> Exists(string corporationId);
    }
}
