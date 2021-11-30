using System.Threading.Tasks;
using System;

namespace ResearchXBRL.Domain.FinancialAnalysis.TimeSeriesAnalysisResults.Corporations
{
    public interface ICorporationRepository
    {
        Task<Corporation> Get(string EdinetCode);
    }
}
