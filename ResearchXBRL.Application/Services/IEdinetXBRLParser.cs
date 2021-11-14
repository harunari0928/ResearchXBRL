using ResearchXBRL.Application.DTO;
using ResearchXBRL.Domain.FinancialReports;

namespace ResearchXBRL.Application.Services
{
    public interface IEdinetXBRLParser
    {
        FinancialReport Parse(EdinetXBRLData data);
    }
}
