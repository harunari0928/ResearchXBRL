using System.Threading.Tasks;
using ResearchXBRL.Application.Services;
using ResearchXBRL.Application.Usecase.ImportAccountItems.Aquire;
using ResearchXBRL.Domain.ImportAccountItems.AccountItems;

namespace ResearchXBRL.Application.Interactors.ImportAccountItems.Aquire
{
    public sealed class AquireAccountItemsInteractor : IAquireAccoumtItemsUsecase
    {
        private readonly ITaxonomyDownloader downloader;
        private readonly ITaxonomyParser parser;
        private readonly IAccountItemsRepository repository;

        public AquireAccountItemsInteractor(
            ITaxonomyDownloader downloader,
            ITaxonomyParser parser,
            IAccountItemsRepository repository)
        {
            this.downloader = downloader;
            this.parser = parser;
            this.repository = repository;
        }

        public async Task Handle()
        {
            await repository.Clean();
            await foreach (var data in downloader.Download())
            {
                var accountElement = parser.Parse(data);
                await repository.Write(accountElement);
            }
        }
    }
}
