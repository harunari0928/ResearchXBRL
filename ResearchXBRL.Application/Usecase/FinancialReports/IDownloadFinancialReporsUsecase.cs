using System;
using System.Threading.Tasks;

namespace ResearchXBRL.Application.Usecase.FinancialReports
{
    public interface IDownloadFinancialReporsUsecase
    {
        Task Handle(DateTimeOffset start, DateTimeOffset end);
    }
}
