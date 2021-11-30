using System.Collections.Generic;

namespace ResearchXBRL.Application.DTO.FinancialAnalysis.AnalysisMenus.AccountItemMenus
{
    public class AccountItemViewModel
    {
        public string Name { get; init; } = "";
        public IReadOnlyList<string> XBRLNames { get; init; } = new string[0];
    }
}
