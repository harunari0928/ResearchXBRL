using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResearchXBRL.Domain.FinancialReports
{
    public interface IFinancialReportReader
    {
        Task<IEnumerable<FinancialReport>> Read(DateTimeOffset start, DateTimeOffset end);
    }
}
