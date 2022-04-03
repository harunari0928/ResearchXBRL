using System.Collections.Generic;

namespace ResearchXBRL.Domain.FinancialAnalysis.AnalysisMenus.AccountItems
{
    public sealed class AccountItemsMenu
    {
        public AccountItem? SearchedAccountItem { get; init; }
        public IReadOnlyList<AccountItem> SuggestedAccountItems { get; init; } = new AccountItem[0];
    }
}
