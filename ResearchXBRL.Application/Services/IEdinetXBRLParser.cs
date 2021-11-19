using ResearchXBRL.Application.DTO;
using ResearchXBRL.Domain.FinancialReports;
using System.Threading.Tasks;

namespace ResearchXBRL.Application.Services
{
    public interface IEdinetXBRLParser
    {
        Task<FinancialReport> Parse(EdinetXBRLData data);
    }
}
