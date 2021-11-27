using System.IO;
using System.Threading.Tasks;

namespace ResearchXBRL.Application.Usecase.AccountElements.Transfer
{
    public interface ITransferAccountElementsUsecase
    {
        Task Hundle(Stream label, Stream schema);
    }
}
