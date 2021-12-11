using System;
using System.Threading.Tasks;
using ResearchXBRL.Application.DTO.FinancialAnalysis.TimeSeriesAnalysis;
using ResearchXBRL.Application.Usecase.FinancialAnalysis.TimeSeriesAnalysis;
using ResearchXBRL.Domain.FinancialAnalysis.TimeSeriesAnalysis;
using ResearchXBRL.Domain.FinancialAnalysis.TimeSeriesAnalysis.Corporations;

namespace ResearchXBRL.Application.Interactors.FinancialAnalysis.TimeSeriesAnalysis
{
    public sealed class PerformTimeSeriesAnalysisInteractor : IPerformTimeSeriesAnalysisUseCase
    {
        private readonly ITimeSeriesAnalysisResultRepository analysisResultRepository;
        private readonly ICorporationRepository corporationRepository;

        public PerformTimeSeriesAnalysisInteractor(
            ITimeSeriesAnalysisResultRepository analysisResultRepository,
            ICorporationRepository corporationRepository)
        {
            this.analysisResultRepository = analysisResultRepository;
            this.corporationRepository = corporationRepository;
        }

        public async Task<TimeSeriesAnalysisViewModel> Handle(AnalyticalMaterials input)
        {
            var corporation = await corporationRepository.Get(input.CorporationId);
            if (corporation is null)
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
}
