using Moq;
using ResearchXBRL.Application.DTO.Results;
using ResearchXBRL.Infrastructure.Services.EdinetXBRLDownloaders;
using RichardSzalay.MockHttp;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace ResearchXBRL.Tests.Infrastructure.Service.EdinetXBRLDownloaders
{
    public sealed class SecurityReportsDownloaderTests
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
                [Fact(DisplayName = "取得開始日から終了日までの全ての日付の書類一覧を取得する")]
                public async Task Test1()
                {
                    // arrange
                    var startDay = new DateTimeOffset(2019, 3, 15, 10, 10, 10, TimeSpan.FromHours(9));
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
                        // 2019/3/15 ~ 2019/4/15の30日間の書類一覧を取得する
                        .Download(startDay, endDay)
                        .ForEachAsync(_ => { });

                    // assert
                    mockHttpHandler.VerifyNoOutstandingExpectation();
                }

                [Fact(DisplayName = "有価証券報告書のみを絞り込む")]
                public async Task Test2()
                {
                    // arrange
                    var startDay = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(9)).AddYears(-3);
                    var endDay = startDay;
                    var downloader = CreateDownloader(mockHttpHandler, "v1");

                    var documentId1 = Guid.NewGuid().ToString();
                    var documentId2 = Guid.NewGuid().ToString();
                    var companyId2 = Guid.NewGuid().ToString();
                    var documentType2 = Guid.NewGuid().ToString();
                    var documentDate = startDay.AddYears(1);
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
            ""formCode"": ""030000"",
            ""docTypeCode"": ""333"",
            ""edinetCode"": ""ccc"",
            ""submitDateTime"": ""{documentDate}""
        }},
        {{
            ""docID"": ""{documentId2}"",
            ""ordinanceCode"": ""010"",
            ""formCode"": ""030000"",
            ""docTypeCode"": ""{documentType2}"",
            ""edinetCode"": ""{companyId2}"",
            ""submitDateTime"": ""{documentDate}""
        }},
        {{
            ""docID"": ""{documentId3}"",
            ""ordinanceCode"": ""010"",
            ""formCode"": ""030001"",
            ""docTypeCode"": ""111"",
            ""edinetCode"": ""aaaa"",
            ""submitDateTime"": ""{documentDate}""
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
                    var data = (await downloader
                        .Download(startDay, endDay)
                        .ToArrayAsync()).Single();

                    // assert
                    if (data is not Succeeded<ResearchXBRL.Application.DTO.EdinetXBRLData> succeeded)
                    {
                        throw new Exception("ダウンロード失敗");
                    }
                    Assert.Equal(documentId2, succeeded.Value.DocumentId);
                    Assert.Equal(documentType2, succeeded.Value.DocumentType);
                    Assert.Equal(companyId2, succeeded.Value.CompanyId);
                    Assert.Equal(documentDate.ToString("yyyy-MM-dd"), new DateTimeOffset(succeeded.Value.DocumentDateTime).ToOffset(TimeSpan.FromHours(9)).ToString("yyyy-MM-dd"));
                }
            }

            public sealed class 異常系 : DownloadTests
            {
                [Fact(DisplayName = "取得開始日が5年よりも前の場合、中断ステータスを返す")]
                public async Task Test1()
                {
                    // arrange
                    var fiveYearsAgo = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(9)).AddYears(-5);
                    var downloader = CreateDownloader(mockHttpHandler, "v1");

                    // act
                    var result = await downloader
                            .Download(fiveYearsAgo, DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(9))).SingleAsync();

                    // assert
                    Assert.IsType<Abort<ResearchXBRL.Application.DTO.EdinetXBRLData>>(result);
                }

                [Fact(DisplayName = "取得終了日が5年よりも前の場合、中断ステータスを返す")]
                public async Task Test2()
                {
                    // arrange
                    var fiveYearsAgo = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(9)).AddYears(-5);
                    var downloader = CreateDownloader(mockHttpHandler, "v1");

                    // act
                    var result = await downloader
                            .Download(DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(9)), fiveYearsAgo)
                            .SingleAsync();

                    // assert
                    Assert.IsType<Abort<ResearchXBRL.Application.DTO.EdinetXBRLData>>(result);
                }

                [Fact(DisplayName = "書類一覧API接続処理失敗時、失敗ステータスを返す")]
                public async Task Test3()
                {
                    // arrange
                    var documentDay = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(9)).AddYears(-3);
                    var downloader = CreateDownloader(mockHttpHandler, "v1");
                    mockHttpHandler
                        .When(HttpMethod.Get,
                        $"https://disclosure.edinet-fsa.go.jp/api/v1/documents.json?date={documentDay:yyyy-MM-dd}&type=2")
                        .Respond(_ => new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.InternalServerError
                        });

                    // act
                    var result = await downloader
                            .Download(documentDay, documentDay)
                            .SingleAsync();

                    // assert
                    Assert.IsType<Failed<ResearchXBRL.Application.DTO.EdinetXBRLData>>(result);
                }

                [Fact(DisplayName = "書類API接続処理に失敗した場合、失敗ステータスを返す")]
                public async Task Test4()
                {
                    // arrange
                    var startDay = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(9)).AddYears(-3);
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
            ""formCode"": ""030000"",
            ""docTypeCode"": ""111"",
            ""edinetCode"": ""aaaa"",
            ""submitDateTime"": ""2021-08-27""
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

                    await foreach (var data in downloader.Download(startDay, startDay).OfType<Succeeded<ResearchXBRL.Application.DTO.EdinetXBRLData>>())
                    {
                        // act
                        var result = await data.Value.LazyZippedDataStream.Value;

                        // assert
                        Assert.IsType<Failed<MemoryStream>>(result);
                    }
                }

                [Fact(DisplayName = "書類一覧API接続処理失敗しても次の取得処理を続行する")]
                public async Task Test5()
                {
                    // arrange
                    var documentDay = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(9)).AddYears(-3);
                    var downloader = CreateDownloader(mockHttpHandler, "v1");
                    mockHttpHandler
                        .When(HttpMethod.Get,
                        $"https://disclosure.edinet-fsa.go.jp/api/v1/documents.json?date={documentDay:yyyy-MM-dd}&type=2")
                        .Respond(_ => new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.InternalServerError
                        });

                    // act
                    var result = await downloader
                            // NOTE: 4日分の書類一覧取得を行う
                            .Download(documentDay, documentDay.AddDays(3))
                            .ToArrayAsync();

                    // assert
                    Assert.Equal(4, result.Length);
                }
            }

            private SecurityReportsDownloader CreateDownloader(MockHttpMessageHandler mockHttpHandler, string apiVersion)
            {
                httpClientFactory
                    .Setup(x => x.CreateClient(typeof(EdinetXBRLDownloader).Name))
                    .Returns(mockHttpHandler.ToHttpClient());
                return new SecurityReportsDownloader(httpClientFactory.Object, apiVersion);
            }
        }
    }
}
