using Moq;
using ResearchXBRL.Infrastructure.Services.EdinetXBRLDownloaders;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ResearchXBRL.Tests.Infrastructure.Service.EdinetXBRLDownloaders
{
    public sealed class SecuritiesReportDownloaderTests
    {
        public class DownloadTests
        {
            private readonly Mock<IHttpClientFactory> httpClientFactory;
            private readonly MockHttpMessageHandler mockHttpHandler;

            public DownloadTests()
            {
                httpClientFactory = new();
                mockHttpHandler = new();
            }

            public sealed class 正常系 : DownloadTests
            {
                [Fact]
                public async Task APIVersionを変えたとき2つのAPIリクエストurlもそれに応じて変わる()
                {
                    // arrange
                    var startDay = new DateTimeOffset(2018, 4, 15, 10, 10, 10, TimeSpan.FromHours(9));
                    var endDay = startDay;
                    var apiVersion = "v114514";
                    var downloader = CreateDownloader(mockHttpHandler, apiVersion);

                    var documentId = Guid.NewGuid().ToString();
                    mockHttpHandler
                        .Expect(HttpMethod.Get,
                        $"https://disclosure.edinet-fsa.go.jp/api/{apiVersion}/documents.json?date={startDay:yyyy-MM-dd}&type=2")
                        .Respond(_ => new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new StringContent($@"
{{
    ""results"": [
        {{
            ""docID"": ""{documentId}"",
            ""ordinanceCode"": ""010"",
            ""formCode"": ""030000""
        }}
    ]
}}")
                        });
                    mockHttpHandler
                        .Expect(HttpMethod.Get,
                        $"https://disclosure.edinet-fsa.go.jp/api/{apiVersion}/documents/{documentId}?type=1")
                        .Respond(_ => new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new StringContent("")
                        });

                    // act
                    await downloader
                        .Download(startDay, endDay)
                        .ForEachAsync(_ => { });

                    // assert
                    mockHttpHandler.VerifyNoOutstandingExpectation();
                }

                [Fact]
                public async Task 取得開始日から終了日までの全ての日付の書類一覧を取得する()
                {
                    // arrange
                    var startDay = new DateTimeOffset(2018, 4, 15, 10, 10, 10, TimeSpan.FromHours(9));
                    var endDay = new DateTimeOffset(2019, 4, 15, 10, 10, 10, TimeSpan.FromHours(9));
                    var downloader = CreateDownloader(mockHttpHandler, "v1");

                    var currentDay = startDay;
                    while (currentDay <= endDay)
                    {
                        mockHttpHandler
                            .Expect(HttpMethod.Get,
                            $"https://disclosure.edinet-fsa.go.jp/api/v1/documents.json?date={currentDay:yyyy-MM-dd}&type=2")
                            .Respond(_ => new HttpResponseMessage
                             {
                                 StatusCode = HttpStatusCode.OK,
                                 Content = new StringContent(@"{ ""results"": [] }")
                             });
                        currentDay = currentDay.AddDays(1);
                    }

                    // act
                    await downloader
                        // 2018/4/15 ~ 2019/4/15の365日間の書類一覧を取得する
                        .Download(startDay, endDay)
                        .ForEachAsync(_ => { });

                    // assert
                    mockHttpHandler.VerifyNoOutstandingExpectation();
                }

                [Fact]
                public async Task 有価証券報告書のみを絞り込む()
                {
                    // arrange
                    var startDay = new DateTimeOffset(2018, 4, 15, 10, 10, 10, TimeSpan.FromHours(9));
                    var endDay = startDay;
                    var downloader = CreateDownloader(mockHttpHandler, "v1");

                    var documentId1 = Guid.NewGuid().ToString();
                    var documentId2 = Guid.NewGuid().ToString();
                    var documentId3 = Guid.NewGuid().ToString();
                    mockHttpHandler
                        .When(HttpMethod.Get,
                        $"https://disclosure.edinet-fsa.go.jp/api/v1/documents.json?date={startDay:yyyy-MM-dd}&type=2")
                        .Respond(_ => new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new StringContent($@"
{{
    ""results"": [
        {{
            ""docID"": ""{documentId1}"",
            ""ordinanceCode"": ""009"",
            ""formCode"": ""030000""
        }},
        {{
            ""docID"": ""{documentId2}"",
            ""ordinanceCode"": ""010"",
            ""formCode"": ""030000""
        }},
        {{
            ""docID"": ""{documentId3}"",
            ""ordinanceCode"": ""010"",
            ""formCode"": ""030001""
        }}
    ]
}}")
                        });
                    mockHttpHandler
                        .When(HttpMethod.Get,
                        $"https://disclosure.edinet-fsa.go.jp/api/v1/documents/*?type=1")
                        .Respond(_ => new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new StringContent("")
                        });

                    // act
                    var data = await downloader
                        .Download(startDay, endDay)
                        .ToArrayAsync();

                    // assert
                    Assert.Equal(documentId2, data.Single().DocumentId);
                }
            }

            public sealed class 異常系 : DownloadTests
            {
                [Fact]
                public async Task 取得開始日が5年よりも前の場合例外を出す()
                {
                    // arrange
                    var fiveYearsAgo = DateTimeOffset.Now.AddYears(-5);
                    var downloader = CreateDownloader(mockHttpHandler, "v1");

                    // act & assert
                    await Assert.ThrowsAsync<ArgumentException>(() =>
                        downloader
                            .Download(fiveYearsAgo, DateTimeOffset.Now)
                            .ForEachAsync(_ => { }));
                }

                [Fact]
                public async Task 取得終了日が5年よりも前の場合例外を出す()
                {
                    // arrange
                    var fiveYearsAgo = DateTimeOffset.Now.AddYears(-5);
                    var downloader = CreateDownloader(mockHttpHandler, "v1");

                    // act & assert
                    await Assert.ThrowsAsync<ArgumentException>(() =>
                        downloader
                            .Download(DateTimeOffset.Now, fiveYearsAgo)
                            .ForEachAsync(_ => { }));
                }

                [Fact]
                public async Task 書類一覧API接続処理失敗時例外を出す()
                {
                    // arrange
                    var documentDay = new DateTimeOffset(2018, 4, 15, 10, 10, 10, TimeSpan.FromHours(9));
                    var downloader = CreateDownloader(mockHttpHandler, "v1");
                    mockHttpHandler
                        .When(HttpMethod.Get,
                        $"https://disclosure.edinet-fsa.go.jp/api/v1/documents.json?date={documentDay:yyyy-MM-dd}&type=2")
                        .Respond(_ => new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.InternalServerError
                        });

                    // act & assert
                    await Assert.ThrowsAsync<HttpRequestException>(() =>
                        downloader
                            .Download(documentDay, documentDay)
                            .ForEachAsync(_ => { }));
                }

                [Fact]
                public async Task 書類API接続処理失敗例外を出す()
                {
                    // arrange
                    var startDay = new DateTimeOffset(2018, 4, 15, 10, 10, 10, TimeSpan.FromHours(9));
                    var endDay = startDay;
                    var apiVersion = "v114514";
                    var downloader = CreateDownloader(mockHttpHandler, apiVersion);

                    var documentId = Guid.NewGuid().ToString();
                    mockHttpHandler
                        .Expect(HttpMethod.Get,
                        $"https://disclosure.edinet-fsa.go.jp/api/{apiVersion}/documents.json?date={startDay:yyyy-MM-dd}&type=2")
                        .Respond(_ => new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new StringContent($@"
{{
    ""results"": [
        {{
            ""docID"": ""{documentId}"",
            ""ordinanceCode"": ""010"",
            ""formCode"": ""030000""
        }}
    ]
}}")
                        });
                    mockHttpHandler
                        .Expect(HttpMethod.Get,
                        $"https://disclosure.edinet-fsa.go.jp/api/{apiVersion}/documents/{documentId}?type=1")
                        .Respond(_ => new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.InternalServerError,
                        });

                    // act & assert
                    await Assert.ThrowsAsync<HttpRequestException>(() =>
                        downloader
                            .Download(startDay, startDay)
                            .ForEachAsync(_ => { }));
                }
            }

            private SecuritiesReportDownloader CreateDownloader(MockHttpMessageHandler mockHttpHandler, string apiVersion)
            {
                httpClientFactory
                    .Setup(x => x.CreateClient(typeof(EdinetXBRLDownloader).Name))
                    .Returns(mockHttpHandler.ToHttpClient());
                return new SecuritiesReportDownloader(httpClientFactory.Object, apiVersion);
            }
        }
    }
}
