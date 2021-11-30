using System.Collections.Generic;

namespace ResearchXBRL.Domain.FinancialAnalysis.AnalysisMenus.AccountItemMenus
{
    public sealed class AccountItem
    {
        public string Name { get; init; } = "";
        public IReadOnlyList<string> XBRLNames { get; init; } = new string[0];
    }
}
