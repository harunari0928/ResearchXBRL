using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using ResearchXBRL.Application.DTO;
using ResearchXBRL.Application.Interactors.AccountElements;
using ResearchXBRL.Application.Services;
using ResearchXBRL.Domain.AccountElements;
using Xunit;

namespace ResearchXBRL.Tests.Application.Interactors.AccountElements
{
    public sealed class AquireAccoumtElementsInteractorTests
    {
        public sealed class HandleTests
        {
            private readonly Mock<ITaxonomyParser> parser;
            private readonly Mock<IAccountElementRepository> repository;
            private readonly Mock<ITaxonomyDownloader> downloader;

            public HandleTests()
            {
                parser = new();
                repository = new();
                downloader = new();
            }

            [Fact]
            public async Task Downloaderから返った値を全て解析する()
            {
                // arrange
                var expectedDownloadResult = new EdinetTaxonomyData[]
                {
                    new EdinetTaxonomyData
                    {
                        LabelDataStream = new MemoryStream(0),
                        SchemaDataStream = new MemoryStream(0),
                    },
                    new EdinetTaxonomyData
                    {
                        LabelDataStream = new MemoryStream(0),
                        SchemaDataStream = new MemoryStream(0),
                    },
                    new EdinetTaxonomyData
                    {
                        LabelDataStream = new MemoryStream(0),
                        SchemaDataStream = new MemoryStream(0),
                    },
                };
                RegisterDownloadResult(expectedDownloadResult.ToAsyncEnumerable());
                var interactor = CreateInteractor();

                // act
                await interactor.Handle();

                // assert
                parser.Verify(x => x.Parse(It.IsAny<EdinetTaxonomyData>()),
                        Times.Exactly(expectedDownloadResult.Length));
            }

            [Fact]
            public async Task Parserから返った値を全て書き込む()
            {
                // arrange
                var expectedDownloadResult = new EdinetTaxonomyData[]
                {
                    new EdinetTaxonomyData
                    {
                        LabelDataStream = new MemoryStream(0),
                        SchemaDataStream = new MemoryStream(0),
                    },
                    new EdinetTaxonomyData
                    {
                        LabelDataStream = new MemoryStream(0),
                        SchemaDataStream = new MemoryStream(0),
                    },
                    new EdinetTaxonomyData
                    {
                        LabelDataStream = new MemoryStream(0),
                        SchemaDataStream = new MemoryStream(0),
                    },
                };
                RegisterDownloadResult(expectedDownloadResult.ToAsyncEnumerable());
                parser
                    .Setup(x => x.Parse(It.IsAny<EdinetTaxonomyData>()))
                    .Returns(Enumerable.Empty<AccountElement>());
                var interactor = CreateInteractor();

                // act
                await interactor.Handle();

                // assert
                repository.Verify(x => x.Write(It.IsAny<IEnumerable<AccountElement>>()),
                        Times.Exactly(expectedDownloadResult.Length));
            }

            private AquireAccoumtElementsInteractor CreateInteractor()
            {
                return new AquireAccoumtElementsInteractor(
                    downloader.Object,
                    parser.Object,
                    repository.Object);
            }

            private void RegisterDownloadResult(IAsyncEnumerable<EdinetTaxonomyData> reports)
            {
                downloader.Setup(x => x.Download()).Returns(reports);
            }
        }
    }
}
