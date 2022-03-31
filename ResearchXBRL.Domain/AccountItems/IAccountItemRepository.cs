using System.Threading.Tasks;

namespace ResearchXBRL.Domain.AccountItems
{
    public interface IAccountItemRepository : IAccountItemWriter
    {
        Task Clean();
    }
}
