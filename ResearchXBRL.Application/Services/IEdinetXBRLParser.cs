using System.Threading.Tasks;
using ResearchXBRL.Application.DTO;
using ResearchXBRL.Domain.ImportFinancialReports.FinancialReports;

namespace ResearchXBRL.Application.Services;

public interface IEdinetXBRLParser
{
    Task<FinancialReport> Parse(EdinetXBRLData data);
}
