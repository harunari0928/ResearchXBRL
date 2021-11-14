using ResearchXBRL.Domain.FinancialReports;
using System;
using System.Collections.Generic;

namespace ResearchXBRL.Application.Usecase.FinancialReports
{
    public interface IDownloadFinancialReporsUsecase
    {
        IAsyncEnumerable<FinancialReport> Handle(DateTimeOffset start, DateTimeOffset end);
    }
}
