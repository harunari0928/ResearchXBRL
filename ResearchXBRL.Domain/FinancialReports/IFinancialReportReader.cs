using System.Collections.Generic;

namespace ResearchXBRL.Domain.FinancialReports
{
    public interface IFinancialReportReader
    {
        IEnumerable<FinancialReport> Read();
    }
}
