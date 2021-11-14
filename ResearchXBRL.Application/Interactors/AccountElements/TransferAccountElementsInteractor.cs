using ResearchXBRL.Application.Usecase.AccountElements.Transfer;
using ResearchXBRL.Domain.AccountElements;
using System.Threading.Tasks;

namespace ResearchXBRL.Application.AccountElements
{
    public sealed class TransferAccountElementsInteractor : ITransferAccountElementsUsecase
    {
        private readonly IAccountElementReader accountElementReader;
        private readonly IAccountElementWriter accountElementWriter;
        private readonly ITransferAccountElementsPresenter presenter;

        public TransferAccountElementsInteractor(
            IAccountElementReader accountElementReader,
            IAccountElementWriter accountElementWriter,
            ITransferAccountElementsPresenter presenter)
        {
            this.accountElementReader = accountElementReader;
            this.accountElementWriter = accountElementWriter;
            this.presenter = presenter;
        }

        public async Task Hundle()
        {
            var accountElements = accountElementReader.Read();
            await accountElementWriter.Write(accountElements);
            presenter.Complete();
        }
    }
}
