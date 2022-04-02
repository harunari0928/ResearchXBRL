using System;
namespace ResearchXBRL.Application.Usecase.ImportFinancialReports
{
    public interface IAquireFinancialReportsPresenter
    {
        void Progress(DateTimeOffset start, DateTimeOffset end, DateTimeOffset current);
        void Complete();
        void Error(string message, Exception ex);
    }
}
