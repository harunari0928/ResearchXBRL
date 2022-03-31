using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Moq.Language.Flow;
using ResearchXBRL.Application.DTO;
using ResearchXBRL.Application.Interactors.AccountItems.Aquire;
using ResearchXBRL.Application.Services;
using ResearchXBRL.Domain.AccountItems;
using Xunit;

namespace ResearchXBRL.Tests.Application.Interactors.AccountItems.Aquire
{
    public sealed class AquireAccoumtItemsInteractorTests
    {
        public sealed class HandleTests
        {
            private readonly Mock<ITaxonomyParser> parser;
            private readonly Mock<IAccountItemRepository> repository;
            private readonly Mock<ITaxonomyDownloader> downloader;

            public HandleTests()
            {
                parser = new();
                repository = new();
                downloader = new();
            }

            [Fact]
            public async Task ダウンロード前に既存全データの削除処理を行う()
            {
                // arrange
                var downloadResult = new EdinetTaxonomyData[]
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
                var isCleaned = false;
                repository.Setup(x => x.Clean())
                    .Callback(() => isCleaned = true);
                RegisterDownloadResult(downloadResult.ToAsyncEnumerable())
                    .Callback(() => Assert.True(isCleaned));

                var interactor = CreateInteractor();

                // act
                await interactor.Handle();

                // assert
                repository.Verify(x => x.Clean(), Times.Once);
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
                    .Returns(Enumerable.Empty<AccountItem>());
                var interactor = CreateInteractor();

                // act
                await interactor.Handle();

                // assert
                repository.Verify(x => x.Write(It.IsAny<IEnumerable<AccountItem>>()),
                        Times.Exactly(expectedDownloadResult.Length));
            }

            private AquireAccountItemsInteractor CreateInteractor()
            {
                return new AquireAccountItemsInteractor(
                    downloader.Object,
                    parser.Object,
                    repository.Object);
            }

            private IReturnsResult<ITaxonomyDownloader> RegisterDownloadResult(IAsyncEnumerable<EdinetTaxonomyData> reports)
            {
                return downloader.Setup(x => x.Download()).Returns(reports);
            }
        }
    }
}
