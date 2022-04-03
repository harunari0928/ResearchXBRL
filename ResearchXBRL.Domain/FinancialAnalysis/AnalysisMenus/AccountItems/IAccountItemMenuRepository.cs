using System.Threading.Tasks;

namespace ResearchXBRL.Domain.FinancialAnalysis.AnalysisMenus.AccountItems
{
    public interface IAccountItemsMenuRepository
    {
        Task<AccountItemsMenu> GetProposals(string keyword);
    }
}
