using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using ResearchXBRL.Infrastructure.Services.EdinetXBRLDownloaders;
using RichardSzalay.MockHttp;
using Xunit;

namespace ResearchXBRL.Tests.Infrastructure.Service.EdinetXBRLDownloaders
{
    public sealed class AllSecurityReportsDownloaderTests
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
                public async Task 有価証券報告書_四半期報告書_半期報告書のみを絞り込む()
                {
                    // arrange
                    var startDay = new DateTimeOffset(2018, 4, 15, 10, 10, 10, TimeSpan.FromHours(9));
                    var endDay = startDay;
                    var downloader = CreateDownloader(mockHttpHandler, "v1");

                    var documentId1 = Guid.NewGuid().ToString();
                    var documentId2 = Guid.NewGuid().ToString();
                    var companyId2 = Guid.NewGuid().ToString();
                    var documentType2 = Guid.NewGuid().ToString();
                    var documentDate = "2021-08-26";
                    var documentId3 = Guid.NewGuid().ToString();
                    var documentId4 = Guid.NewGuid().ToString();
                    var documentId5 = Guid.NewGuid().ToString();
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
            ""submitDateTime"": ""2021-08-25""
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
            ""submitDateTime"": ""2021-08-27""
        }},
        {{
            ""docID"": ""{documentId4}"",
            ""ordinanceCode"": ""010"",
            ""formCode"": ""043000"",
            ""docTypeCode"": ""140"",
            ""edinetCode"": ""aaaa2"",
            ""submitDateTime"": ""2021-08-27""
        }},
        {{
            ""docID"": ""{documentId5}"",
            ""ordinanceCode"": ""010"",
            ""formCode"": ""050000"",
            ""docTypeCode"": ""160"",
            ""edinetCode"": ""aaaa3"",
            ""submitDateTime"": ""2021-08-27""
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
                    Assert.Equal(3, data.Length);
                    Assert.Contains(documentId4, data.Select(x => x.DocumentId));
                    Assert.Contains(documentId5, data.Select(x => x.DocumentId));
                }
            }

            private AllSecurityReportsDownloader CreateDownloader(MockHttpMessageHandler mockHttpHandler, string apiVersion)
            {
                httpClientFactory
                    .Setup(x => x.CreateClient(typeof(EdinetXBRLDownloader).Name))
                    .Returns(mockHttpHandler.ToHttpClient());
                return new AllSecurityReportsDownloader(httpClientFactory.Object, apiVersion);
            }
        }
    }
}
