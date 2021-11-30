using System.Threading.Tasks;

namespace ResearchXBRL.Domain.FinancialAnalysis.AnalysisMenus.CorporationMenus
{
    public interface ICorporationMenuRepository
    {
        Task<CorporatonMenu> GetProposals(string keyword);
    }
}
