using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResearchXBRL.Domain.FinancialAnalysis.AnalysisMenus
{
    public interface IAnalysisMenuRepository
    {
        Task<AnalysisMenu> Get();
    }
}
