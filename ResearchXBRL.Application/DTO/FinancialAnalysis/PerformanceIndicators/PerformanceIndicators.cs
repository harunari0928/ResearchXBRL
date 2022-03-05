using System.Collections.Generic;
using ResearchXBRL.Application.DTO.FinancialAnalysis.PerformanceIndicators.Indicators;

namespace ResearchXBRL.Application.DTO.FinancialAnalysis.PerformanceIndicators;

public sealed class PerformanceIndicator
{
    public IReadOnlyList<Indicator> Indicators { get; init; } = new List<Indicator>();
}
