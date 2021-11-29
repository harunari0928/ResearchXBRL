using System.Collections.Generic;
using System;

namespace ResearchXBRL.Domain.FinancialAnalysis.AnalysisMenus
{
    public sealed class AnalysisMenu
    {
        public IReadOnlyList<AccountItem> AccountItems { get; init; } = new AccountItem[0];
        public IReadOnlyList<Corporation> Corporations { get; init; } = new Corporation[0];
    }
}
