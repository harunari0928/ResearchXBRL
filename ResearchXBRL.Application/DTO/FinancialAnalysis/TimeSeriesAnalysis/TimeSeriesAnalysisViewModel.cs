namespace ResearchXBRL.Application.DTO.FinancialAnalysis.TimeSeriesAnalysis
{
    public sealed class TimeSeriesAnalysisViewModel
    {
        public string AccountName { get; init; } = "";

        public object Unit { get; init; } = new object();

        public object Corporation { get; init; } = new object();

        public object Values { get; init; } = new object();
    }
}
