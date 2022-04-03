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
            private readonly LocalFileStorage storage;

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
            public void 指定したパスのストリームを取得する_引数パスの先頭がスラッシュのとき()
            {
                // arrange
                using var stream = new MemoryStream();
                using var writer = new StreamWriter(stream) { AutoFlush = true };
                var expectedStr = "テストです";
                writer.WriteLine(expectedStr);
                var filePath = $"./{Guid.NewGuid()}/testd/test.txt";
                storage.Set(stream, filePath);

                // act
                var outputStream = storage.Get(string.Concat(filePath.Skip(1)));

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
            private readonly LocalFileStorage storage;

            public GetFilesTests()
            {
                storage = new(basePath);
            }

            public void Dispose()
            {
                Clean(basePath);
            }

            [Fact]
            public void 指定したパスの全ファイルパスを取得する()
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
            public void ベースパスからの相対パスを返す()
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
                var files = storage.GetFiles(path);

                // assert
                Assert.True(Enumerable.Range(0, fileSize)
                    .Select(n => $"test{n}.txt")
                    .Zip(files, (expected, actual) => actual.StartsWith(path))
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
            private readonly LocalFileStorage storage;

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
            private readonly LocalFileStorage storage;

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

        public sealed class DeleteTests : IDisposable
        {
            private readonly string basePath = $"./{Guid.NewGuid()}";
            private readonly LocalFileStorage storage;

            public DeleteTests()
            {
                storage = new(basePath);
            }

            public void Dispose()
            {
                Clean(basePath);
            }

            [Fact]
            public void 指定したディレクトリを削除する()
            {
                // arrange
                using var stream = new MemoryStream();
                using var writer = new StreamWriter(stream) { AutoFlush = true };
                var expectedStr = "テストです";
                writer.WriteLine(expectedStr);
                var filePath = $"./{Guid.NewGuid()}/testd/test.txt";
                var directoryPath = filePath.Replace("test.txt", "");
                storage.Set(stream, filePath);

                // act
                storage.Delete(directoryPath);

                // assert
                Assert.False(Directory.Exists(Path.Combine(basePath, directoryPath)));
            }

            [Fact]
            public void 指定したディレクトリが存在しないとき例外を出す()
            {
                // arrange
                var directoryPath = $"./{Guid.NewGuid()}/testd/";

                // act & assert
                Assert.Throws<DirectoryNotFoundException>(() => storage.Delete(directoryPath));
            }

            [Fact]
            public void 指定したファイルを削除する()
            {
                // arrange
                using var stream = new MemoryStream();
                using var writer = new StreamWriter(stream) { AutoFlush = true };
                var expectedStr = "テストです";
                writer.WriteLine(expectedStr);
                var filePath = $"./{Guid.NewGuid()}/testd/test.txt";
                storage.Set(stream, filePath);

                // act
                storage.Delete(filePath);

                // assert
                Assert.False(File.Exists(Path.Combine(basePath, filePath)));
            }

            [Fact]
            public void 指定したファイルが存在しないとき例外を出す()
            {
                // act & assert
                Assert.Throws<FileNotFoundException>(() => storage.Delete("./not_existed_file.csv"));
            }
        }

        public sealed class GetDirectoryNamesTests : IDisposable
        {
            private readonly string basePath = $"./{Guid.NewGuid()}";
            private readonly LocalFileStorage storage;

            public GetDirectoryNamesTests()
            {
                storage = new(basePath);
            }

            public void Dispose()
            {
                Clean(basePath);
            }

            [Fact]
            public void 指定したパスの全ディレクトリ名を取得する()
            {
                // arrange
                var path = "./testd";
                var folderSize = 10;
                foreach (var number in Enumerable.Range(0, folderSize))
                {
                    using var stream = new MemoryStream();
                    using var writer = new StreamWriter(stream) { AutoFlush = true };
                    stream.Position = 0;
                    var expectedStr = $"テストです{number}";
                    writer.WriteLine(expectedStr);
                    var filePath = $"{path}/testd{number}/test{number}.txt";
                    storage.Set(stream, filePath);
                }

                // act
                var directoryNames = storage.GetDirectoryNames(path).OrderBy(x => x);

                // assert
                Assert.Equal(folderSize, directoryNames.Count());
                Assert.True(Enumerable.Range(0, folderSize)
                    .Select(n => $"testd{n}")
                    .Zip(directoryNames, (expected, actual) => actual == expected)
                    .All(x => x));
            }

            [Fact]
            public void 引数で指定したパターンのディレクトリ名を取得する()
            {
                // arrange
                var path = "./testd";
                var folderSize = 10;
                foreach (var number in Enumerable.Range(0, folderSize))
                {
                    using var stream = new MemoryStream();
                    using var writer = new StreamWriter(stream) { AutoFlush = true };
                    stream.Position = 0;
                    var expectedStr = $"テストです{number}";
                    writer.WriteLine(expectedStr);
                    var filePath = $"{path}/testd{number}/test{number}.txt";
                    storage.Set(stream, filePath);
                }

                // act
                var directoryName = storage.GetDirectoryNames(path, "testd5").Single();

                // assert
                Assert.Equal("testd5", directoryName);
            }

            [Fact]
            public void ファイルパスが指定されたとき例外を出す()
            {
                // act & assert
                Assert.Throws<ArgumentException>(() => storage.GetDirectoryNames("/test/test.txt"));
            }
        }

        private static void Clean(string path)
        {
            Directory.Delete(path, true);
        }
    }
}
