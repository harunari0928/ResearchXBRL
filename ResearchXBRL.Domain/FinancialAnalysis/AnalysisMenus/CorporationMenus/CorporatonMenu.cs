using System.Collections.Generic;
using System;

namespace ResearchXBRL.Domain.FinancialAnalysis.AnalysisMenus.CorporationMenus
{
    public sealed class CorporatonMenu
    {
        public IReadOnlyList<Corporation> Corporations { get; init; } = new Corporation[0];
    }
}
