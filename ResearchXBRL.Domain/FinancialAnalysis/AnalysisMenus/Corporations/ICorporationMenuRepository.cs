using System.Threading.Tasks;

namespace ResearchXBRL.Domain.FinancialAnalysis.AnalysisMenus.Corporations
{
    public interface ICorporationMenuRepository
    {
        Task<CorporatonMenu> GetProposals(string keyword);
    }
}
