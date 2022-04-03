using System.Threading.Tasks;

namespace ResearchXBRL.Domain.ImportFinancialReports.FinancialReports;

public interface IFinancialReportsRepository
{
    Task<bool> IsExists(string doucmentId);
    Task Write(FinancialReport reports);
}
