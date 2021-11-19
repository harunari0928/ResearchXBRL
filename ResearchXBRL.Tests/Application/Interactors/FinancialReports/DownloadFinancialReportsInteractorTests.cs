using Moq;
using ResearchXBRL.Application.DTO;
using ResearchXBRL.Application.FinancialReports;
using ResearchXBRL.Application.Services;
using ResearchXBRL.Domain.FinancialReportItems;
using ResearchXBRL.Domain.FinancialReports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ResearchXBRL.Tests.Application.Interactors.FinancialReports
{
    public sealed class DownloadFinancialReportsInteractorTests
    {
        public class HandleTests
        {
            private readonly Mock<IEdinetXBRLDownloader> downloader;
            private readonly Mock<IEdinetXBRLParser> parser;
            private readonly Mock<IFinancialReportRepository> reportRepository;

            public HandleTests()
            {
                downloader = new();
                parser = new();
                reportRepository = new();
            }

            public sealed class 正常系 : HandleTests
            {
                [Fact]
                public async Task 引数と同じ値をDonwloaderへ渡す()
                {
                    // arrange
                    RegisterDownloadResult(AsyncEnumerable.Empty<EdinetXBRLData>());
                    var interactor = CreateInteractor();
                    var (expectedStart, expectedEnd) = (new DateTimeOffset(2019, 11, 24, 14, 15, 1, TimeSpan.FromHours(9)),
                        new DateTimeOffset(2021, 1, 3, 4, 5, 10, TimeSpan.FromHours(9)));

                    // act
                    await interactor
                        .Handle(expectedStart, expectedEnd);

                    // assert
                    downloader
                        .Verify(x => x.Download(expectedStart, expectedEnd),
                        Times.Once);
                }

                [Fact]
                public async Task Downloaderから返った値を全て解析する()
                {
                    // arrange
                    var expectedDownloadResult = new EdinetXBRLData[]
                    {
                        new EdinetXBRLData
                        {
                            DocumentId = Guid.NewGuid().ToString(),
                        },
                        new EdinetXBRLData
                        {
                            DocumentId = Guid.NewGuid().ToString(),
                        },
                        new EdinetXBRLData
                        {
                            DocumentId = Guid.NewGuid().ToString(),
                        },
                    };
                    RegisterDownloadResult(expectedDownloadResult.ToAsyncEnumerable());
                    var interactor = CreateInteractor();

                    // act
                    await interactor
                        .Handle(DateTimeOffset.Now, DateTimeOffset.Now);

                    // assert
                    parser.Verify(x => x.Parse(It.IsAny<EdinetXBRLData>()),
                            Times.Exactly(expectedDownloadResult.Length));
                }

                [Fact]
                public async Task Parserから返った値を全て書き込む()
                {
                    // arrange
                    var expectedDownloadResult = new EdinetXBRLData[]
                    {
                        new EdinetXBRLData
                        {
                            DocumentId = Guid.NewGuid().ToString(),
                        },
                        new EdinetXBRLData
                        {
                            DocumentId = Guid.NewGuid().ToString(),
                        },
                        new EdinetXBRLData
                        {
                            DocumentId = Guid.NewGuid().ToString(),
                        },
                    };
                    RegisterDownloadResult(expectedDownloadResult.ToAsyncEnumerable());
                    parser.Setup(x => x.Parse(It.IsAny<EdinetXBRLData>()))
                        .ReturnsAsync(new FinancialReport(Enumerable.Empty<FinancialReportItem>()));
                    var interactor = CreateInteractor();

                    // act
                    await interactor
                        .Handle(DateTimeOffset.Now, DateTimeOffset.Now);

                    // assert
                    reportRepository.Verify(x => x.Write(It.IsAny<FinancialReport>()),
                            Times.Exactly(expectedDownloadResult.Length));
                }
            }

            public sealed class 異常系 : HandleTests
            {
                [Fact]
                public async Task 引数のstartよりもendが前の場合例外を出す()
                {
                    // arrange
                    RegisterDownloadResult(AsyncEnumerable.Empty<EdinetXBRLData>());
                    var interactor = CreateInteractor();
                    var (start, end) = (new DateTimeOffset(2019, 11, 24, 14, 15, 1, TimeSpan.FromHours(9)),
                        new DateTimeOffset(2019, 11, 24, 14, 15, 0, TimeSpan.FromHours(9)));

                    // act & assert
                    await Assert.ThrowsAsync<ArgumentException>(()
                        => interactor.Handle(start, end));
                }
            }

            private AquireFinancialReportsInteractor CreateInteractor()
            {
                return new AquireFinancialReportsInteractor(
                    downloader.Object,
                    parser.Object,
                    reportRepository.Object);
            }

            private void RegisterDownloadResult(IAsyncEnumerable<EdinetXBRLData> reports)
            {
                downloader
                    .Setup(x => x.Download(
                        It.IsAny<DateTimeOffset>(),
                        It.IsAny<DateTimeOffset>()))
                    .Returns(reports);
            }
        }
    }
}
