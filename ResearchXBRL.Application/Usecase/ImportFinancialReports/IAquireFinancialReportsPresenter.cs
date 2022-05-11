using System;
namespace ResearchXBRL.Application.Usecase.ImportFinancialReports
{
    public interface IAquireFinancialReportsPresenter
    {
        void Start();
        void Progress(DateTimeOffset start, DateTimeOffset end, DateTimeOffset current);
        void Complete();
        void Warn(string message);
        void Error(string message, Exception ex);
        void Error(string message);
    }
}
