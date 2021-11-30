using System.Threading.Tasks;
using System;

namespace ResearchXBRL.Domain.FinancialAnalysis.TimeSeriesAnalysis.Corporations
{
    public interface ICorporationRepository
    {
        Task<Corporation> Get(string EdinetCode);
    }
}
