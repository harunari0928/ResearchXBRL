using System.Threading.Tasks;

namespace ResearchXBRL.Domain.FinancialAnalysis.TimeSeriesAnalysis.Corporations
{
    public interface ICorporationRepository
    {
        Task<bool> Exists(string corporationId);
    }
}
