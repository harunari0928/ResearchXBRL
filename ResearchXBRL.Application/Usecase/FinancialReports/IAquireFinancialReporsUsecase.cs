using System;
using System.Threading.Tasks;

namespace ResearchXBRL.Application.Usecase.FinancialReports
{
    public interface IAquireFinancialReporsUsecase
    {
        Task Handle(DateTimeOffset start, DateTimeOffset end);
    }
}
