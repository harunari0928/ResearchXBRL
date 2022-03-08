using System.Threading.Tasks;

namespace ResearchXBRL.Domain.FinancialAnalysis.AnalysisMenus.Corporations
{
    public interface ICorporationsMenuRepository
    {
        Task<CorporatonsMenu> GetProposals(string keyword);
        ValueTask<Corporation?> FindBySecuritiesCode(string securitiesCode);
    }
}
