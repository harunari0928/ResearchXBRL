using System.Threading.Tasks;

namespace ResearchXBRL.Domain.FinancialAnalysis.TimeSeriesAnalysis.Corporations
{
    public interface ICorporationsRepository
    {
        ValueTask<bool> Exists(string corporationId);
    }
}
