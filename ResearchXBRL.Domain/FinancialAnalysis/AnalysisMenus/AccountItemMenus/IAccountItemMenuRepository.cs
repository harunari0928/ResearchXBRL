using System.Threading.Tasks;

namespace ResearchXBRL.Domain.FinancialAnalysis.AnalysisMenus.AccountItemMenus
{
    public interface IAccountItemMenuRepository
    {
        Task<AccountItemMenu> GetProposals(string keyword);
    }
}
