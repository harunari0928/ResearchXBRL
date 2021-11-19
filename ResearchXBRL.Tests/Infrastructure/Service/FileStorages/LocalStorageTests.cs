using ResearchXBRL.Infrastructure.Services.FileStorages;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Xunit;

namespace ResearchXBRL.Tests.Infrastructure.Service.FileStorages
{
    public sealed class LocalStorageTests
    {
        public sealed class GetTests : IDisposable
        {
            private readonly string basePath = $"./{Guid.NewGuid()}";
            private readonly LocalStorage storage;

            public GetTests()
            {
                storage = new(basePath);
            }

            public void Dispose()
            {
                Clean(basePath);
            }

            [Fact]
            public void 指定したパスのストリームを取得する()
            {
                // arrange
                using var stream = new MemoryStream();
                using var writer = new StreamWriter(stream) { AutoFlush = true };
                var expectedStr = "テストです";
                writer.WriteLine(expectedStr);
                var filePath = $"./{Guid.NewGuid()}/testd/test.txt";
                storage.Set(stream, filePath);

                // act
                var outputStream = storage.Get(filePath);

                // assert
                using var reader = new StreamReader(outputStream);
                Assert.Equal(expectedStr, reader.ReadLine());
            }

            [Fact]
            public void ディレクトリパスが指定されたとき例外を出す()
            {
                // act & assert
                Assert.Throws<IOException>(() => storage.Get("/test"));
            }
        }

        public sealed class GetFilesTests : IDisposable
        {
            private readonly string basePath = $"./{Guid.NewGuid()}";
            private readonly LocalStorage storage;

            public GetFilesTests()
            {
                storage = new(basePath);
            }

            public void Dispose()
            {
                Clean(basePath);
            }

            [Fact]
            public void 指定したパスの全ファイル名を取得する()
            {
                // arrange
                var path = "./testd";
                var fileSize = 10;
                foreach (var number in Enumerable.Range(0, fileSize))
                {
                    using var stream = new MemoryStream();
                    using var writer = new StreamWriter(stream) { AutoFlush = true };
                    stream.Position = 0;
                    var expectedStr = $"テストです{number}";
                    writer.WriteLine(expectedStr);
                    var filePath = $"{path}/test{number}.txt";
                    storage.Set(stream, filePath);
                }

                // act
                var files = storage.GetFiles(path).OrderBy(x => x);

                // assert
                Assert.Equal(fileSize, files.Count());
                Assert.True(Enumerable.Range(0, fileSize)
                    .Select(n => $"test{n}.txt")
                    .Zip(files, (expected, actual) => actual.Contains(expected))
                    .All(x => x));
            }

            [Fact]
            public void ファイルパスが指定されたとき例外を出す()
            {
                // act & assert
                Assert.Throws<IOException>(() => storage.GetFiles("/test/test.txt"));
            }
        }

        public sealed class SetTests : IDisposable
        {
            private readonly string basePath = $"./{Guid.NewGuid()}";
            private readonly LocalStorage storage;

            public SetTests()
            {
                storage = new(basePath);
            }

            public void Dispose()
            {
                Clean(basePath);
            }

            [Fact]
            public void ディレクトリパスが指定されたとき例外を出す()
            {
                // arrange
                using var stream = new MemoryStream();
                using var writer = new StreamWriter(stream) { AutoFlush = true };
                var expectedStr = "テストです";
                writer.WriteLine(expectedStr);

                // act & assert
                Assert.Throws<IOException>(() => storage.Set(stream, "/testd"));
            }
        }

        public sealed class UnzipTests : IDisposable
        {
            private readonly string basePath = $"./{Guid.NewGuid()}";
            private readonly LocalStorage storage;

            public UnzipTests()
            {
                storage = new(basePath);
            }

            public void Dispose()
            {
                Clean(basePath);
            }

            [Fact]
            public void 指定したzipファイルを解凍する()
            {
                // arrange
                using var stream = new MemoryStream();
                using var writer = new StreamWriter(stream) { AutoFlush = true };
                var expectedStr = "テストです";
                writer.WriteLine(expectedStr);
                var directoryPath = $"./{Guid.NewGuid()}/testd";
                var filePath = $"{directoryPath}/test.txt";
                storage.Set(stream, filePath);
                var zipFilePath = "./test.zip";
                ZipFile.CreateFromDirectory(Path.Combine(basePath, directoryPath), Path.Combine(basePath, zipFilePath));
                var unzippedDirectoryPath = $"./{Guid.NewGuid()}/unzipped";

                // act
                storage.Unzip(zipFilePath, unzippedDirectoryPath);

                // assert
                Assert.True(Directory.Exists(Path.Combine(basePath, unzippedDirectoryPath)));
            }

            [Fact]
            public void 解凍後ZIPファイルを消す()
            {
                // arrange
                using var stream = new MemoryStream();
                using var writer = new StreamWriter(stream) { AutoFlush = true };
                var expectedStr = "テストです";
                writer.WriteLine(expectedStr);
                var directoryPath = $"./{Guid.NewGuid()}/testd";
                var filePath = $"{directoryPath}/test.txt";
                storage.Set(stream, filePath);
                var zipFilePath = "./test.zip";
                ZipFile.CreateFromDirectory(Path.Combine(basePath, directoryPath), Path.Combine(basePath, zipFilePath));
                var unzippedDirectoryPath = $"./{Guid.NewGuid()}/unzipped";

                // act
                storage.Unzip(zipFilePath, unzippedDirectoryPath);

                // assert
                Assert.False(Directory.Exists(Path.Combine(basePath, zipFilePath)));
            }

            [Fact]
            public void ZIPのファイルパスがディレクトリパスのとき例外を出す()
            {
                // act & assert
                Assert.Throws<IOException>(() => storage.Unzip("/test", "/test2"));
            }

            [Fact]
            public void 解凍後のディレクトリパスがファイルパスのとき例外を出す()
            {
                // act & assert
                Assert.Throws<IOException>(() => storage.Unzip("/test/text.txt", "/test2/file.txt"));
            }
        }

        private static void Clean(string path)
        {
            Directory.Delete(path, true);
        }
    }
}
