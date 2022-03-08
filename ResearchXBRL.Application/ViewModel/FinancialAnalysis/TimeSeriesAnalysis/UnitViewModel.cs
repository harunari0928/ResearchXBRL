namespace ResearchXBRL.Application.ViewModel.FinancialAnalysis.TimeSeriesAnalysis
{
    public sealed class UnitViewModel
    {
        public string Name { get; init; } = "";
        public string? Measure { get; init; }
        public string? UnitNumerator { get; init; }
        public string? UnitDenominator { get; init; }
    }
}
