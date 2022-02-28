using System;
using System.Collections.Generic;
using ResearchXBRL.Application.DTO.FinancialAnalysis.PerformanceIndicators.Indicators;

namespace ResearchXBRL.Application.DTO.FinancialAnalysis.PerformanceIndicators
{
    public class PerformanceIndicatorsViewModel
    {
        public IReadOnlyList<IndicatorViewModel> Indicators { get; init; } = new List<IndicatorViewModel>();
    }
}
