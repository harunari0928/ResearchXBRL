using System.Collections.Generic;
using ResearchXBRL.Domain.FinancialAnalysis.PerformanceIndicators.Indicators;

namespace ResearchXBRL.Domain.FinancialAnalysis.PerformanceIndicators
{
    public sealed class PerformanceIndicators
    {
        public IReadOnlyList<Indicator> Indicators { get; init; } = new List<Indicator>();
    }
}
