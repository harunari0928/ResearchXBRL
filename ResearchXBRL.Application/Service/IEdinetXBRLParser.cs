using ResearchXBRL.Application.DTO;
using ResearchXBRL.Domain.FinancialReports;

namespace ResearchXBRL.Application.Service
{
    public interface IEdinetXBRLParser
    {
        FinancialReport Parse(EdinetXBRLData data);
    }
}
