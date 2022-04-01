using ResearchXBRL.Application.DTO;
using ResearchXBRL.Application.Services;
using ResearchXBRL.Application.Usecase.ImportAccountItems.Transfer;
using ResearchXBRL.Domain.ImportAccountItems.AccountItems;
using System.IO;
using System.Threading.Tasks;

namespace ResearchXBRL.Application.Interactors.ImportAccountItems.Transfer
{
    public sealed class TransferAccountItemsInteractor : ITransferAccountItemsUsecase
    {
        private readonly ITaxonomyParser accountElementReader;
        private readonly IAccountItemsWriter accountElementWriter;
        private readonly ITransferAccountItemsPresenter presenter;

        public TransferAccountItemsInteractor(
            in ITaxonomyParser accountElementReader,
            in IAccountItemsWriter accountElementWriter,
            in ITransferAccountItemsPresenter presenter)
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
