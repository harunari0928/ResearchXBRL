using System;
using System.Threading.Tasks;
using ResearchXBRL.Application.ViewModel.FinancialAnalysis.PerformanceIndicators;
using ResearchXBRL.Application.Usecase.FinancialAnalysis.PerformanceIndicators;
using ResearchXBRL.Application.QueryServices.FinancialAnalysis.PerformanceIndicators;
using ResearchXBRL.Application.DTO.FinancialAnalysis.PerformanceIndicators.Indicators;

namespace ResearchXBRL.Application.Interactors.FinancialAnalysis.PerformanceIndicators;

public sealed class GetPerformanceIndicatorsInteractor : IGetPerformanceIndicatorsUsecase
{
    private readonly ICorporationsQueryService corporationsQueryService;
    private readonly IPerformanceIndicatorsQueryService indicatorsQueryService;
    private readonly ITimeseriesAccountValuesQueryService timeseriesAccountValuesQueryService;

    public GetPerformanceIndicatorsInteractor(
        ICorporationsQueryService corporationQueryService,
        IPerformanceIndicatorsQueryService indicatorsQueryService,
        ITimeseriesAccountValuesQueryService timeseriesAccountValuesQueryService)
    {
        this.corporationsQueryService = corporationQueryService;
        this.indicatorsQueryService = indicatorsQueryService;
        this.timeseriesAccountValuesQueryService = timeseriesAccountValuesQueryService;
    }

    public async ValueTask<PerformanceIndicatorViewModel> Handle(string corporationId)
    {
        if (!await corporationsQueryService.Exists(corporationId))
        {
            throw new ArgumentException("指定された企業は存在しません");
        }

        var performanceIndicator = await indicatorsQueryService.Get(corporationId);

        if (performanceIndicator.Exists(IndicatorType.RateOfReturnOnEquitySummaryOfBusinessResults))
        {
            var profitOrLossValues = await timeseriesAccountValuesQueryService.Get(corporationId, "当期純利益又は当期純損失（△）");
            var capitalValues = await timeseriesAccountValuesQueryService.Get(corporationId, "資本金");
            performanceIndicator.UpdateROEValue(profitOrLossValues, capitalValues);
        }

        return new PerformanceIndicatorViewModel(performanceIndicator);
    }
}
