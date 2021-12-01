using System.Threading.Tasks;
using ResearchXBRL.Application.DTO.FinancialAnalysis.TimeSeriesAnalysis;

namespace ResearchXBRL.Application.Usecase.FinancialAnalysis.TimeSeriesAnalysis
{
    public interface IPerformTimeSeriesAnalysisUseCase
    {
        Task<TimeSeriesAnalysisViewModel> Handle(AnalyticalMaterials input);
    }
}
