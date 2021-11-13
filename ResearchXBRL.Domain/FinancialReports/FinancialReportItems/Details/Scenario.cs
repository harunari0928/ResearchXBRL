using System.Collections.Generic;

namespace ResearchXBRL.Domain.FinancialReports.FinancialReportItems.Details
{
    public sealed class Scenario
    {
        /// <summary>
        /// 所属するディメンジョン情報
        /// </summary>
        public IReadOnlyList<DemensionInfo> DemensionInfosToBelonging { get; init; }
    }
}
