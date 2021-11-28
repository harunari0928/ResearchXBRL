using System.Threading.Tasks;
using ResearchXBRL.Application.Services;
using ResearchXBRL.Application.Usecase.AccountElements;
using ResearchXBRL.Domain.AccountElements;

namespace ResearchXBRL.Application.Interactors.AccountElements
{
    public sealed class AquireAccoumtElementsInteractor : IAquireAccoumtElementsUsecase
    {
        private readonly ITaxonomyDownloader downloader;
        private readonly ITaxonomyParser parser;
        private readonly IAccountElementRepository repository;

        public AquireAccoumtElementsInteractor(
            ITaxonomyDownloader downloader,
            ITaxonomyParser parser,
            IAccountElementRepository repository)
        {
            this.downloader = downloader;
            this.parser = parser;
            this.repository = repository;
        }

        public async Task Handle()
        {
            await foreach (var data in downloader.Download())
            {
                var accountElement = parser.Parse(data);
                await repository.Write(accountElement);
            }
        }
    }
}
