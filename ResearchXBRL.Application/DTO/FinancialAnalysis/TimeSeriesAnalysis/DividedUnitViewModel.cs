namespace ResearchXBRL.Application.DTO.FinancialAnalysis.TimeSeriesAnalysis
{
    public sealed class DividedUnitViewModel : IUnitViewModel
    {
        public string Name { get; init; } = "";
        public string UnitNumerator { get; init; } = "";
        public string UnitDenominator { get; init; } = "";
    }
}
