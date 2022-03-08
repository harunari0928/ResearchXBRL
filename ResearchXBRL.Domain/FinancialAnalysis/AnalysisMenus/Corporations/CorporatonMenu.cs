using System.Collections.Generic;

namespace ResearchXBRL.Domain.FinancialAnalysis.AnalysisMenus.Corporations;

public sealed class CorporatonsMenu
{
    public IReadOnlyList<Corporation> Corporations { get; init; } = new Corporation[0];
}
