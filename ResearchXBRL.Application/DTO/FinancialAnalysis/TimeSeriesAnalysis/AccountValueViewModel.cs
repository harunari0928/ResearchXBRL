namespace ResearchXBRL.Application.DTO.FinancialAnalysis.TimeSeriesAnalysis
{
    public class AccountValueViewModel
    {
        public IAccountsPeriodViewModel FinancialAccountPeriod { get; init; } = new InstantPeriodViewModel();
        public decimal Amount { get; init; }
    }
}
