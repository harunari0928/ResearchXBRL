using System.Threading.Tasks;

namespace ResearchXBRL.Domain.ImportAccountItems.AccountItems;

public interface IAccountItemsRepository : IAccountItemsWriter
{
    Task Clean();
}
