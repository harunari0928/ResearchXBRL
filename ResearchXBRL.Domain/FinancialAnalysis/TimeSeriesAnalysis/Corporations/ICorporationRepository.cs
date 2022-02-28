using System.Threading.Tasks;

namespace ResearchXBRL.Domain.FinancialAnalysis.TimeSeriesAnalysis.Corporations
{
    public interface ICorporationRepository
    {
        ValueTask<bool> Exists(string corporationId);
    }
}
