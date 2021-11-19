using ResearchXBRL.Domain.FinancialReports;
using System.Threading.Tasks;

namespace ResearchXBRL.Infrastructure.FinancialReports
{
    public sealed class FinancialReportRepository : IFinancialReportRepository
    {
        public Task<bool> IsExists(FinancialReport reports)
        {
            throw new System.NotImplementedException();
        }

        public Task Write(FinancialReport reports)
        {
            throw new System.NotImplementedException();
        }
    }
}
