using System;
using System.Collections.Generic;

namespace ResearchXBRL.Application.DTO.FinancialAnalysis.PerformanceIndicators.Indicators
{
    public class IndicatorViewModel
    {
        public string IndicatorName { get; init; } = "";

        public IReadOnlyDictionary<DateOnly, decimal> Values { get; init; } = new Dictionary<DateOnly, decimal>();
    }
}
