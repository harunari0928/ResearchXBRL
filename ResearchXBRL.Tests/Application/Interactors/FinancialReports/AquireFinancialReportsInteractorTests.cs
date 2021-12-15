using Moq;
using ResearchXBRL.Application.DTO;
using ResearchXBRL.Application.FinancialReports;
using ResearchXBRL.Application.Services;
using ResearchXBRL.Application.Usecase.FinancialReports;
using ResearchXBRL.Domain.FinancialReportItems;
using ResearchXBRL.Domain.FinancialReports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ResearchXBRL.Tests.Application.Interactors.FinancialReports
{
    public sealed class AquireFinancialReportsInteractorTests
    {
        public class HandleTests
        {
            private readonly Mock<IEdinetXBRLDownloader> downloader;
            private readonly Mock<IEdinetXBRLParser> parser;
            private readonly Mock<IFinancialReportRepository> reportRepository;
            private readonly Mock<IAquireFinancialReportsPresenter> presenter;

            public HandleTests()
            {
                downloader = new();
                parser = new();
                reportRepository = new();
                presenter = new();
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
                    var interactor = CreateInteractor();

                    // act
                    await interactor
                        .Handle(DateTimeOffset.Now, DateTimeOffset.Now);

                    // assert
                    reportRepository.Verify(x => x.Write(It.IsAny<FinancialReport>()),
                            Times.Exactly(expectedDownloadResult.Length));
                }

                [Fact]
                public async Task 既に財務データが保存されていれば解析及び書き込みを行わない()
                {
                    // arrange
                    var existedDocumentId = Guid.NewGuid().ToString();
                    var expectedDownloadResult = new EdinetXBRLData[]
                    {
                        new EdinetXBRLData
                        {
                            DocumentId = existedDocumentId,
                        },
                    };
                    RegisterDownloadResult(expectedDownloadResult.ToAsyncEnumerable());
                    reportRepository
                        .Setup(x => x.IsExists(existedDocumentId))
                        .ReturnsAsync(true);
                    var interactor = CreateInteractor();

                    // act
                    await interactor
                        .Handle(DateTimeOffset.Now, DateTimeOffset.Now);

                    // assert
                    parser.Verify(x => x.Parse(It.IsAny<EdinetXBRLData>()),
                            Times.Never);
                    reportRepository.Verify(x => x.Write(It.IsAny<FinancialReport>()),
                            Times.Never);
                }

                [Fact]
                public async Task 書き込みの都度に進捗報告を行う()
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
                    presenter.Verify(x => x.Progress(
                        It.IsAny<DateTimeOffset>(),
                        It.IsAny<DateTimeOffset>(),
                        It.IsAny<DateTimeOffset>()),
                        Times.Exactly(expectedDownloadResult.Length));
                }

                [Fact]
                public async Task 正常終了したとき完了通知を行う()
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
                    presenter.Verify(x => x.Complete(), Times.Once);
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

                [Fact]
                public async Task 解析処理で例外が発生したとき例外が発生する()
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
                    parser
                        .Setup(x => x.Parse(It.IsAny<EdinetXBRLData>()))
                        // 3回の全ての解析処理で下記例外が発生するとする
                        .ThrowsAsync(new OutOfMemoryException());
                    var interactor = CreateInteractor();

                    // act & assert
                    var exception = await Assert.ThrowsAsync<AggregateException>(()
                        => interactor.Handle(DateTimeOffset.Now, DateTimeOffset.Now));

                    Assert.Equal(
                        expectedDownloadResult.Length,
                        exception.InnerExceptions.OfType<OutOfMemoryException>().Count());
                    presenter.Verify(x => x.Error(It.Is<string>(x => x.Contains("インポート中にエラーが発生しました")),
                        It.IsAny<OutOfMemoryException>()),
                        Times.Exactly(expectedDownloadResult.Length), "エラーが出る都度に通知する");
                    presenter.Verify(x => x.Complete(), Times.Never, "完了通知は行わない");
                }
            }

            private AquireFinancialReportsInteractor CreateInteractor()
            {
                return new AquireFinancialReportsInteractor(
                    downloader.Object,
                    parser.Object,
                    reportRepository.Object,
                    presenter.Object,
                    2);
            }

            private void RegisterDownloadResult(IAsyncEnumerable<EdinetXBRLData> reports)
            {
                downloader
                    .Setup(x => x.Download(
                        It.IsAny<DateTimeOffset>(),
                        It.IsAny<DateTimeOffset>()))
                    .Returns(reports);
                parser
                    .Setup(x => x.Parse(It.IsAny<EdinetXBRLData>()))
                    .ReturnsAsync(new FinancialReport(Enumerable.Empty<FinancialReportItem>())
                    {
                        Cover = new ReportCover
                        {
                            SubmissionDate = DateTimeOffset.Now
                        }
                    });
            }
        }
    }
}
