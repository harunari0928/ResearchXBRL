namespace ResearchXBRL.Domain.FinancialReports.Units
{
    /// <summary>
    /// 分母及び分子を用いたユニット
    /// </summary>
    public sealed class DividedUnit : IUnit
    {
        public string Name { get; init; }
        /// <summary>
        /// 分子
        /// </summary>
        public string UnitNumerator { get; init; }
        /// <summary>
        /// 分母
        /// </summary>
        public string UnitDenominator { get; init; }
    }
}
