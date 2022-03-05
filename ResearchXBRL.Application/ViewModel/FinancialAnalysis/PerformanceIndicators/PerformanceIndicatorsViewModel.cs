using System.Linq;
using System.Collections.Generic;
using ResearchXBRL.Application.ViewModel.FinancialAnalysis.PerformanceIndicators.Indicators;
using ResearchXBRL.Application.DTO.FinancialAnalysis.PerformanceIndicators;

namespace ResearchXBRL.Application.ViewModel.FinancialAnalysis.PerformanceIndicators;

public class PerformanceIndicatorViewModel
{
    public IReadOnlyList<IndicatorViewModel> Indicators { get; init; } = new List<IndicatorViewModel>();

    public PerformanceIndicatorViewModel() { }

    public PerformanceIndicatorViewModel(in PerformanceIndicator domainModel)
    {
        Indicators = CovertIndicators(domainModel).ToList();
    }

    private static IEnumerable<IndicatorViewModel> CovertIndicators(PerformanceIndicator domainModel)
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
