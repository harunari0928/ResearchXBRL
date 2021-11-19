using System.Threading.Tasks;

namespace ResearchXBRL.Domain.FinancialReports
{
    public interface IFinancialReportRepository
    {
        Task<bool> IsExists(FinancialReport reports);
        Task Write(FinancialReport reports);
    }
}
