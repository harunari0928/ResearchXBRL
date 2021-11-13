namespace ResearchXBRL.Domain.FinancialReports.FinancialReportItems.Units
{
    /// <summary>
    /// 分母及び分子を用いたユニット
    /// </summary>
    public sealed class DividedUnit : IUnit
    {
        /// <summary>
        /// 分子
        /// </summary>
        public string UnitNumeratorMeasure { get; init; }
        /// <summary>
        /// 分母
        /// </summary>
        public string UnitDenominator { get; init; }
    }
}
