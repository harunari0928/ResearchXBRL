using System.Threading.Tasks;

namespace ResearchXBRL.Domain.FinancialReports
{
    public interface IFinancialReportRepository
    {
        Task<bool> IsExists(string doucmentId);
        Task Write(FinancialReport reports);
    }
}
