using System.Collections.Generic;

namespace ResearchXBRL.Domain.FinancialAnalysis.AnalysisMenus.AccountItemMenus
{
    public sealed class AccountItemMenu
    {
        public IReadOnlyList<AccountItem> AccountItems { get; init; } = new AccountItem[0];
    }
}
