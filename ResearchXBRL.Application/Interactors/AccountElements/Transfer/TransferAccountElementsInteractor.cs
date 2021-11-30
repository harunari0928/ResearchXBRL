using ResearchXBRL.Application.DTO;
using ResearchXBRL.Application.Services;
using ResearchXBRL.Application.Usecase.AccountElements.Transfer;
using ResearchXBRL.Domain.AccountElements;
using System.IO;
using System.Threading.Tasks;

namespace ResearchXBRL.Application.Interactors.AccountElements.Transfer
{
    public sealed class TransferAccountElementsInteractor : ITransferAccountElementsUsecase
    {
        private readonly ITaxonomyParser accountElementReader;
        private readonly IAccountElementWriter accountElementWriter;
        private readonly ITransferAccountElementsPresenter presenter;

        public TransferAccountElementsInteractor(
            in ITaxonomyParser accountElementReader,
            in IAccountElementWriter accountElementWriter,
            in ITransferAccountElementsPresenter presenter)
        {
            this.accountElementReader = accountElementReader;
            this.accountElementWriter = accountElementWriter;
            this.presenter = presenter;
        }

        public async Task Hundle(Stream label, Stream schema)
        {
            var accountElements = accountElementReader.Parse(new EdinetTaxonomyData
            {
                LabelDataStream = label,
                SchemaDataStream = schema
            });
            await accountElementWriter.Write(accountElements);
            presenter.Complete();
        }
    }
}
