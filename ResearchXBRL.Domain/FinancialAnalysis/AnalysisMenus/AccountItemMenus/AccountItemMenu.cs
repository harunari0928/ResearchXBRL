using System.Collections.Generic;

namespace ResearchXBRL.Domain.FinancialAnalysis.AnalysisMenus.AccountItemMenus
{
    public sealed class AccountItemMenu
    {
        public AccountItem? SearchedAccountItem { get; init; }
        public IReadOnlyList<AccountItem> SuggestedAccountItems { get; init; } = new AccountItem[0];
    }
}
