namespace ResearchXBRL.Application.Usecase.FinancialReports
{
    public interface IAquireFinancialReportsPresenter
    {
        void Progress(int percentage);
        void Complete();
    }
}
