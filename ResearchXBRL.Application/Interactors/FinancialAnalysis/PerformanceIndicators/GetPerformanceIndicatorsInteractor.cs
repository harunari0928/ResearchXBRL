using System;
using System.Threading.Tasks;
using ResearchXBRL.Application.ViewModel.FinancialAnalysis.PerformanceIndicators;
using ResearchXBRL.Application.Usecase.FinancialAnalysis.PerformanceIndicators;
using ResearchXBRL.Application.QueryServices.FinancialAnalysis.PerformanceIndicators;

namespace ResearchXBRL.Application.Interactors.FinancialAnalysis.PerformanceIndicators;

public sealed class GetPerformanceIndicatorsInteractor : IGetPerformanceIndicatorsUsecase
{
    private readonly ICorporationsQueryService corporationsQueryService;
    private readonly IPerformanceIndicatorQueryService indicatorsQueryService;

    public GetPerformanceIndicatorsInteractor(
        ICorporationsQueryService corporationQueryService,
        IPerformanceIndicatorQueryService indicatorsQueryService)
    {
        this.corporationsQueryService = corporationQueryService;
        this.indicatorsQueryService = indicatorsQueryService;
    }

    public async ValueTask<PerformanceIndicatorViewModel> Handle(string corporationId)
    {
        if (!await corporationsQueryService.Exists(corporationId))
        {
            throw new ArgumentException("指定された企業は存在しません");
        }

        var performanceIndicators = await indicatorsQueryService.Get(corporationId);

        return new PerformanceIndicatorViewModel(performanceIndicators);
    }
}
