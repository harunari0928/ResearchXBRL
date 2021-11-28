using System.Threading.Tasks;

namespace ResearchXBRL.Domain.AccountElements
{
    public interface IAccountElementRepository : IAccountElementWriter
    {
        Task Clean();
    }
}
