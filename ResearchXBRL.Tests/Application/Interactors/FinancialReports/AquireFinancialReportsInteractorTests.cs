﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using ResearchXBRL.Application.DTO;
using ResearchXBRL.Application.DTO.Results;
using ResearchXBRL.Application.ImportFinancialReports;
using ResearchXBRL.Application.Services;
using ResearchXBRL.Application.Usecase.ImportFinancialReports;
using ResearchXBRL.Domain.ImportFinancialReports.FinancialReportItems;
using ResearchXBRL.Domain.ImportFinancialReports.FinancialReports;

using Xunit;

namespace ResearchXBRL.Tests.Application.Interactors.FinancialReports;

public sealed class AquireFinancialReportsInteractorTests
{
    public class HandleTests
    {
        private readonly Mock<IEdinetXBRLDownloader> downloader;
        private readonly Mock<IEdinetXBRLParser> parser;
        private readonly Mock<IFinancialReportsRepository> reportRepository;
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
                    .Handle(GetValidateResult(expectedStart, expectedEnd));

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
                    .Handle(GetValidateResult(DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(9)), DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(9))));

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
                    .Handle(GetValidateResult(DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(9)), DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(9))));

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
                    .Handle(GetValidateResult(DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(9)), DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(9))));

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
                    .Handle(GetValidateResult(DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(9)), DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(9))));

                // assert
                presenter.Verify(x => x.Progress(
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<DateTimeOffset>()),
                    Times.Exactly(expectedDownloadResult.Length));
            }

            [Fact]
            public async Task 処理開始時開始ログを出す終了したとき完了通知を行う()
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

                using (var interactor = CreateInteractor())
                {
                    // assert
                    presenter.Verify(x => x.Start(), Times.Once);

                    // act
                    await interactor
                        .Handle(GetValidateResult(DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(9)), DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(9))));
                }

                // assert
                presenter.Verify(x => x.Complete(), Times.Once);
            }
        }

        public sealed class 異常系 : HandleTests
        {
            [Fact]
            public async Task 引数のstartよりもendが前の場合エラーログを出す()
            {
                // arrange
                RegisterDownloadResult(AsyncEnumerable.Empty<EdinetXBRLData>());
                var interactor = CreateInteractor();
                var (start, end) = (new DateTimeOffset(2019, 11, 24, 14, 15, 1, TimeSpan.FromHours(9)),
                    new DateTimeOffset(2019, 11, 24, 14, 15, 0, TimeSpan.FromHours(9)));

                // act
                await interactor.Handle(GetValidateResult(start, end));

                // assert
                presenter.Verify(x => x.Warn(It.IsAny<string>()), Times.Once);
            }

            [Fact]
            public async Task 解析処理で例外が発生したときエラーログを出す()
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

                // act
                await interactor.Handle(GetValidateResult(DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(9)), DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(9))));

                // assert
                presenter.Verify(x => x.Error(It.Is<string>(x => x.Contains("インポート中にエラーが発生しました")),
                    It.IsAny<OutOfMemoryException>()),
                    Times.Exactly(expectedDownloadResult.Length), "エラーが出る都度に通知する");
                presenter.Verify(x => x.Error(It.IsAny<string>()), Times.Once, "最後にエラー件数エラーログを出す");
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
                .Returns(reports.Select(x => new Succeeded<EdinetXBRLData>(x)));
            parser
                .Setup(x => x.Parse(It.IsAny<EdinetXBRLData>()))
                .ReturnsAsync(new FinancialReport(Enumerable.Empty<FinancialReportItem>())
                {
                    Cover = new ReportCover
                    {
                        SubmissionDate = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(9))
                    }
                });
        }

        private static IResult<(DateTimeOffset, DateTimeOffset)> GetValidateResult(DateTimeOffset from, DateTimeOffset to)
        {
            return new Succeeded<(DateTimeOffset, DateTimeOffset)>((from, to));
        }
    }
}
