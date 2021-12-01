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
            if (corporation.IsLinking)
            {
                return await GetNonConsolidateResult(input);
            }

            return await GetConsolidateResult(input);
        }

        private async Task<TimeSeriesAnalysisViewModel> GetConsolidateResult(AnalyticalMaterials input)
        {
            var nonConsolidateResult = await analysisResultRepository.GetNonConsolidateResult(
                            input.CorporationId,
                            input.AccountItemName);
            return new TimeSeriesAnalysisViewModel(nonConsolidateResult);
        }
        private async Task<TimeSeriesAnalysisViewModel> GetNonConsolidateResult(AnalyticalMaterials input)
        {
            var consolidateResult = await analysisResultRepository.GetConsolidateResult(
                                    input.CorporationId,
                                    input.AccountItemName);
            return new TimeSeriesAnalysisViewModel(consolidateResult);
        }
    }
}
