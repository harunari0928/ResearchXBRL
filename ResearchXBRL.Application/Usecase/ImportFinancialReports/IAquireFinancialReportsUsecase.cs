using System;
using System.Threading.Tasks;
using ResearchXBRL.Application.DTO.Results;

namespace ResearchXBRL.Application.Usecase.ImportFinancialReports
{
    public interface IAquireFinancialReportsUsecase
    {
        Task Handle(IResult<(DateTimeOffset, DateTimeOffset)> mayBeFromTo);
    }
}
