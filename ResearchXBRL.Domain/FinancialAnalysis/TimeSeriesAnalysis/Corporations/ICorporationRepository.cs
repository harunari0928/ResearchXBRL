using System.Threading.Tasks;

namespace ResearchXBRL.Domain.FinancialAnalysis.TimeSeriesAnalysis.Corporations
{
    public interface ICorporationRepository
    {
        Task<Corporation?> Get(string corporationId);
    }
}
