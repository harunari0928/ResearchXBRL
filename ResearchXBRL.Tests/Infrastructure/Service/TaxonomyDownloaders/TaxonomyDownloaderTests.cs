using System.Net.Http;
using Xunit;
using Moq;
using RichardSzalay.MockHttp;
using ResearchXBRL.Infrastructure.Services.TaxonomyDownloaders;
using ResearchXBRL.Infrastructure.Services;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace ResearchXBRL.Tests.Infrastructure.Service.TaxonomyDownloaders
{
    public sealed class TaxonomyDownloaderTests
    {
        public class DownloadTests
        {
            private readonly Mock<IHttpClientFactory> httpClientFactory;
            private readonly Mock<IFileStorage> storage;
            private readonly MockHttpMessageHandler mockHttpHandler;

            public DownloadTests()
            {
                httpClientFactory = new();
                storage = new();
                mockHttpHandler = new();
            }

            public sealed class 正常系 : DownloadTests
            {
                [Fact]
                public async Task ダウンロードしたファイル内の全てのバージョンのタクソノミを出力する()
                {
                    // arrange
                    // ダウンロードしたファイル内に存在するタクソノミバージョン
                    var expectedVersions = new string[]
                    {
                        "2014-03-31",
                        "2017-02-28",
                        "2019-11-01",
                    };
                    storage
                        .Setup(x => x.GetDirectoryNames("/unzipped/data/EDINET/taxonomy",
                            It.IsAny<string>()))
                        .Returns(expectedVersions);
                    foreach (var version in expectedVersions)
                    {
                        storage
                          .Setup(x => x.GetDirectoryNames(Path.Combine("/unzipped/data/EDINET/taxonomy/", version, "/taxonomy/"),
                              It.IsAny<string>()))
                          // 全てのバージョンにおいて以下3つの分類が存在するとする
                          .Returns(new string[] { "jpcor", "jppfs", "jpigp" });
                    }
                    mockHttpHandler
                        .When(HttpMethod.Get,
                        $"http://lang.main.jp/xbrl/data.zip")
                        .Respond(_ => new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK
                        });
                    var downloader = CreateDownloader();

                    // act
                    var data = downloader.Download();

                    // assert
                    var acutal1 = data // 分類: jpcorに関して全てのバージョンが存在することを確認
                        .Where(x => x.Classification == "jpcor")
                        .Select(x => $"{x.TaxonomyVersion:yyyy-MM-dd}");
                    Assert.True(await acutal1.SequenceEqualAsync(expectedVersions.ToAsyncEnumerable()));
                    var acutal2 = data // 分類: jppfsに関して全てのバージョンが存在することを確認
                        .Where(x => x.Classification == "jppfs")
                        .Select(x => $"{x.TaxonomyVersion:yyyy-MM-dd}");
                    Assert.True(await acutal2.SequenceEqualAsync(expectedVersions.ToAsyncEnumerable()));
                    var acutal3 = data // 分類: jpigpに関して全てのバージョンが存在することを確認
                        .Where(x => x.Classification == "jpigp")
                        .Select(x => $"{x.TaxonomyVersion:yyyy-MM-dd}");
                    Assert.True(await acutal3.SequenceEqualAsync(expectedVersions.ToAsyncEnumerable()));
                }

                [Fact]
                public async Task 存在しない分類は出力しない()
                {
                    // arrange
                    // ダウンロードしたファイル内に存在するタクソノミバージョン
                    var expectedVersions = new string[]
                    {
                        "2014-03-31",
                        "2019-11-01",
                    };
                    storage
                        .Setup(x => x.GetDirectoryNames("/unzipped/data/EDINET/taxonomy",
                            It.IsAny<string>()))
                        .Returns(expectedVersions);
                    foreach (var version in expectedVersions)
                    {
                        storage
                          .Setup(x => x.GetDirectoryNames(Path.Combine("/unzipped/data/EDINET/taxonomy/", version, "/taxonomy/"),
                              It.IsAny<string>()))
                          // 全てのバージョンにおいてjpigpが存在しないとする
                          .Returns(new string[] { "jpcor", "jppfs" });
                    }
                    mockHttpHandler
                        .When(HttpMethod.Get,
                        $"http://lang.main.jp/xbrl/data.zip")
                        .Respond(_ => new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK
                        });
                    var downloader = CreateDownloader();

                    // act
                    var data = downloader.Download();

                    // assert
                    Assert.False(await data.AnyAsync(x => x.Classification == "jpigp"));
                }
            }

            public sealed class 異常系 : DownloadTests
            {
                [Fact]
                public async Task ダウンロード失敗時例外を出す()
                {
                    // arrange
                    var downloader = CreateDownloader();

                    // act & assert
                    await Assert.ThrowsAnyAsync<HttpRequestException>(()
                        => downloader.Download()
                        .ForEachAsync(_ => { }));
                }
            }

            private TaxonomyDownloader CreateDownloader()
            {
                httpClientFactory
                    .Setup(x => x.CreateClient(typeof(TaxonomyDownloader).Name))
                    .Returns(mockHttpHandler.ToHttpClient());
                return new TaxonomyDownloader(httpClientFactory.Object, storage.Object);
            }
        }
    }
}
