using System.IO;
using System.Threading.Tasks;

namespace ResearchXBRL.Application.Usecase.AccountItems.Transfer
{
    public interface ITransferAccountItemsUsecase
    {
        Task Hundle(Stream label, Stream schema);
    }
}
