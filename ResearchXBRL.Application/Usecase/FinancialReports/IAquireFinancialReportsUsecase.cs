using System;
using System.Threading.Tasks;

namespace ResearchXBRL.Application.Usecase.FinancialReports
{
    public interface IAquireFinancialReportsUsecase
    {
        Task Handle(DateTimeOffset start, DateTimeOffset end);
    }
}
