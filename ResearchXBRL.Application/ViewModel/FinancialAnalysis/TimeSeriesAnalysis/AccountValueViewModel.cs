namespace ResearchXBRL.Application.ViewModel.FinancialAnalysis.TimeSeriesAnalysis;

public class AccountValueViewModel
{
    public AccountsPeriodViewModel FinancialAccountPeriod { get; init; } = new AccountsPeriodViewModel();
    public decimal Amount { get; init; }
}
