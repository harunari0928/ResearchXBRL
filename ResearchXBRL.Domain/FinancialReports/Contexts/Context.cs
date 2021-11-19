namespace ResearchXBRL.Domain.FinancialReports.Details
{
    public sealed class Context
    {
        public string Name { get; init; }
        public IPeriod Period { get; init; }
        public Scenario Scenario { get; init; }
    }
}
