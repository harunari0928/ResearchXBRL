using System.Collections.Generic;

namespace ResearchXBRL.Domain.FinancialAnalysis.AnalysisMenus.Corporations;

public sealed class CorporatonMenu
{
    public IReadOnlyList<Corporation> Corporations { get; init; } = new Corporation[0];
}
