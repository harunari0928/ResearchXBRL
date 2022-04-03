using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using ResearchXBRL.Infrastructure.Services;
using ResearchXBRL.Infrastructure.Services.TaxonomyDownloaders;
using RichardSzalay.MockHttp;
using Xunit;

namespace ResearchXBRL.Tests.Infrastructure.Service.TaxonomyDownloaders;

public class TaxonomyDownloaderTests
{
    private readonly Mock<IHttpClientFactory> httpClientFactory;
    private readonly Mock<IFileStorage> storage;
    private readonly MockHttpMessageHandler mockHttpHandler;

    public TaxonomyDownloaderTests()
    {
        httpClientFactory = new();
        storage = new();
        storage
            .Setup(x => x.Get(in It.Ref<string>.IsAny))
            .Returns(new MemoryStream());
        mockHttpHandler = new();
    }

    public sealed class DownloadTests
    {
        public sealed class 正常系 : TaxonomyDownloaderTests
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
                    var classifications = new string[] { "jpcrp", "jppfs", "jpigp" };
                    storage
                      .Setup(x => x.GetDirectoryNames($"/unzipped/data/EDINET/taxonomy/{version}/taxonomy/",
                          It.IsAny<string>()))
                      // 全てのバージョンにおいて以下3つの分類が存在するとする
                      .Returns(classifications);
                    foreach (var classification in classifications)
                    {
                        storage
                           .Setup(x => x.GetDirectoryNames($"/unzipped/data/EDINET/taxonomy/{version}/taxonomy/{classification}",
                               It.IsAny<string>()))
                           .Returns(new string[] { version });
                    }
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
            public void 内閣府令は取得しない()
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
                    var classifications = new string[] { "jpcrp", "jppfs", "jpigp" };
                    storage
                      .Setup(x => x.GetDirectoryNames($"/unzipped/data/EDINET/taxonomy/{version}/taxonomy/",
                          It.IsAny<string>()))
                      // 全てのバージョンにおいて以下3つの分類が存在するとする
                      .Returns(classifications);
                    foreach (var classification in classifications)
                    {
                        storage
                           .Setup(x => x.GetDirectoryNames($"/unzipped/data/EDINET/taxonomy/{version}/taxonomy/{classification}",
                               It.IsAny<string>()))
                           .Returns(new string[] { version });
                    }
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
                var acutal1 = data // 分類: jpcrpは取得しない
                    .Where(x => x.Classification == "jpcrp")
                    .Select(x => $"{x.TaxonomyVersion:yyyy-MM-dd}");
                Assert.Empty(acutal1.ToEnumerable());
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
                    var classifications = new string[] { "jpcrp", "jppfs" };
                    storage
                      .Setup(x => x.GetDirectoryNames($"/unzipped/data/EDINET/taxonomy/{version}/taxonomy/",
                          It.IsAny<string>()))
                      // 全てのバージョンにおいてjpigpが存在しないとする
                      .Returns(classifications);
                    foreach (var classification in classifications)
                    {
                        storage
                           .Setup(x => x.GetDirectoryNames($"/unzipped/data/EDINET/taxonomy/{version}/taxonomy/{classification}",
                               It.IsAny<string>()))
                           .Returns(new string[] { version });
                    }
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

            [Fact]
            public async Task あるバージョンのタクソノミフォルダにいずれかの分類が存在した場合でも古いバージョンだった場合は出力しない()
            {
                // arrange
                // ダウンロードしたファイル内に存在するタクソノミバージョン
                var expectedVersions = "2014-03-31";
                storage
                    .Setup(x => x.GetDirectoryNames("/unzipped/data/EDINET/taxonomy",
                        It.IsAny<string>()))
                    .Returns(new string[] { expectedVersions });
                storage
                    .Setup(x => x.GetDirectoryNames($"/unzipped/data/EDINET/taxonomy/{expectedVersions}/taxonomy/",
                        It.IsAny<string>()))
                    // バージョン2014-03-31においてjpcrpが存在するとする
                    .Returns(new string[] { "jpcrp" });
                storage
                    .Setup(x => x.GetDirectoryNames($"/unzipped/data/EDINET/taxonomy/{expectedVersions}/taxonomy/jpcrp",
                        It.IsAny<string>()))
                    // バージョン2014-03-31のjpcrpの中に、古いバージョンが存在するとする
                    .Returns(new string[] { "2013-02-28" });

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
                Assert.Empty(await data.ToArrayAsync());
            }
        }

        public sealed class 異常系 : TaxonomyDownloaderTests
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
    }

    public class DisposeTests : TaxonomyDownloaderTests
    {
        [Fact]
        public void IDisposableを継承している()
        {
            // arrange
            var downloader = CreateDownloader();

            // act & assert
            Assert.IsAssignableFrom<IDisposable>(downloader);
        }

        [Fact]
        public void ダウンロードしたファイルを削除する()
        {
            // arrange
            storage
                .Setup(x => x.GetDirectoryNames(".", "*"))
                .Returns(new string[] { "work", "unzipped" });
            var downloader = CreateDownloader();

            // act
            downloader.Dispose();

            // assert
            storage
                .Verify(x => x.Delete("work"), Times.Once
                , "zipファイル格納ディレクトリを削除する");
            storage
                .Verify(x => x.Delete("unzipped"), Times.Once
                , "zip解凍後ディレクトリを削除する");
        }

        [Fact]
        public void 削除対象ディレクトリが存在しないとき削除しない()
        {
            // arrange
            storage
                .Setup(x => x.GetDirectoryNames(".", "*"))
                .Returns(Enumerable.Empty<string>().ToArray());
            var downloader = CreateDownloader();

            // act
            downloader.Dispose();

            // assert
            storage
                .Verify(x => x.Delete("work"), Times.Never);
            storage
                .Verify(x => x.Delete("unzipped"), Times.Never);
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
