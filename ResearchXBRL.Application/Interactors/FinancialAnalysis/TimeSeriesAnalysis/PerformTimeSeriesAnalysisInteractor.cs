using System;
using System.Threading.Tasks;
using ResearchXBRL.Application.ViewModel.FinancialAnalysis.TimeSeriesAnalysis;
using ResearchXBRL.Application.Usecase.FinancialAnalysis.TimeSeriesAnalysis;
using ResearchXBRL.Domain.FinancialAnalysis.TimeSeriesAnalysis;
using ResearchXBRL.Domain.FinancialAnalysis.TimeSeriesAnalysis.Corporations;

namespace ResearchXBRL.Application.Interactors.FinancialAnalysis.TimeSeriesAnalysis;
public sealed class PerformTimeSeriesAnalysisInteractor : IPerformTimeSeriesAnalysisUsecase
{
    private readonly ITimeSeriesAnalysisResultRepository analysisResultRepository;
    private readonly ICorporationsRepository corporationRepository;

    public PerformTimeSeriesAnalysisInteractor(
        ITimeSeriesAnalysisResultRepository analysisResultRepository,
        ICorporationsRepository corporationRepository)
    {
        this.analysisResultRepository = analysisResultRepository;
        this.corporationRepository = corporationRepository;
    }

    public async Task<TimeSeriesAnalysisViewModel> Handle(AnalyticalMaterials input)
    {
        var corporationExists = await corporationRepository.Exists(input.CorporationId);
        if (!corporationExists)
        {
            throw new ArgumentException("指定された企業は存在しません");
        }

        return await GetConsolidateResult(input);
    }

    private async Task<TimeSeriesAnalysisViewModel> GetConsolidateResult(AnalyticalMaterials input)
    {
        var nonConsolidateResult = await analysisResultRepository.GetResult(
                        input.CorporationId,
                        input.AccountItemName);
        return new TimeSeriesAnalysisViewModel(nonConsolidateResult);
    }
}
