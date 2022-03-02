using System.Linq;
using System.Collections.Generic;
using ResearchXBRL.Application.DTO.FinancialAnalysis.PerformanceIndicators.Indicators;

namespace ResearchXBRL.Application.DTO.FinancialAnalysis.PerformanceIndicators
{
    public class PerformanceIndicatorsViewModel
    {
        public IReadOnlyList<IndicatorViewModel> Indicators { get; init; } = new List<IndicatorViewModel>();

        public PerformanceIndicatorsViewModel() { }

        public PerformanceIndicatorsViewModel(in ResearchXBRL.Domain.FinancialAnalysis.PerformanceIndicators.PerformanceIndicators domainModel)
        {
            Indicators = CovertIndicators(domainModel).ToList();
        }

        private static IEnumerable<IndicatorViewModel> CovertIndicators(Domain.FinancialAnalysis.PerformanceIndicators.PerformanceIndicators domainModel)
        {
            foreach (var indicator in domainModel.Indicators)
            {
                yield return new IndicatorViewModel
                {
                    IndicatorType = (IndicatorTypeViewModel)indicator.IndicatorType,
                    Values = indicator.Values.ToDictionary(x => x.Key.ToDateTime(System.TimeOnly.MinValue), y => y.Value)
                };
            }
        }
    }
}
