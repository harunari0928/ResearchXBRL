using System;
using System.IO;
using Moq;
using Renci.SshNet;
using ResearchXBRL.Infrastructure.Shared.FileStorages;
using Xunit;

namespace ResearchXBRL.Tests.Infrastructure.Shared.FileStorages;

public sealed class SFTPFileStorageTests
{

    public sealed class GetTests
    {
        private readonly Mock<ISftpClient> sftpClientMock = new();

        [Fact(DisplayName = "指定したファイルが存在した場合そのファイルの読み出しを行う")]
        public void Test1()
        {
            // arrange
            sftpClientMock.Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
            var fileStorage = new SFTPFileStorage(sftpClientMock.Object, "");

            // act
            var acutal = fileStorage.Get("test");

            // assert
            sftpClientMock.Verify(x => x.OpenRead(It.IsAny<string>()), Times.Once);
        }

        [Fact(DisplayName = "指定したファイルが存在しない場合読み出しを行わず、nullを返す")]
        public void Test2()
        {
            // arrange
            sftpClientMock.Setup(x => x.Exists(It.IsAny<string>())).Returns(false);
            var fileStorage = new SFTPFileStorage(sftpClientMock.Object, "");

            // act
            var acutal = fileStorage.Get("test");

            // assert
            Assert.Null(acutal);
            sftpClientMock.Verify(x => x.OpenRead(It.IsAny<string>()), Times.Never);
        }

        [Fact(DisplayName = "指定したファイルパスにベースパスを付与してクライアントにリクエストする")]
        public void Test3()
        {
            // arrange
            var baseFilePath = "base";
            sftpClientMock.Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
            var fileStorage = new SFTPFileStorage(sftpClientMock.Object, baseFilePath);

            // act
            var acutal = fileStorage.Get("test");

            // assert
            sftpClientMock.Verify(x => x.OpenRead($"{baseFilePath}/test"), Times.Once);
        }
    }

    public sealed class SetTests
    {
        private readonly Mock<ISftpClient> sftpClientMock = new();

        [Fact(DisplayName = "入力されたStreamのアップロードを行う")]
        public void Test1()
        {
            // arrange
            var stream = new MemoryStream();
            var fileStorage = new SFTPFileStorage(sftpClientMock.Object, "");
            var actionResultMock = new Mock<IAsyncResult>();
            actionResultMock.SetupGet(x => x.IsCompleted).Returns(true);
            sftpClientMock
                .Setup(x => x.BeginUploadFile(It.IsAny<Stream>(), It.IsAny<string>()))
                .Returns(actionResultMock.Object);

            // act
            fileStorage.Set(stream, "test");

            // assert
            sftpClientMock.Verify(x => x.BeginUploadFile(stream, "test"), Times.Once);
        }

        [Fact(DisplayName = "引数にベースパスを付与したディレクトリにStreamをアップロードする")]
        public void Test2()
        {
            // arrange
            var baseFilePath = "base";
            var stream = new MemoryStream();
            var fileStorage = new SFTPFileStorage(sftpClientMock.Object, baseFilePath);
            var actionResultMock = new Mock<IAsyncResult>();
            actionResultMock.SetupGet(x => x.IsCompleted).Returns(true);
            sftpClientMock
                .Setup(x => x.BeginUploadFile(It.IsAny<Stream>(), It.IsAny<string>()))
                .Returns(actionResultMock.Object);

            // act
            fileStorage.Set(stream, "test");

            // assert
            sftpClientMock.Verify(x => x.BeginUploadFile(stream, $"{baseFilePath}/test"), Times.Once);
        }
    }

    public sealed class CreateFileTests
    {
        private readonly Mock<ISftpClient> sftpClientMock = new();

        [Fact(DisplayName = "指定したファイルパスにベースパスを付与してクライアントにリクエストする")]
        public void Test1()
        {
            // arrange
            var baseFilePath = "base";
            var fileStorage = new SFTPFileStorage(sftpClientMock.Object, baseFilePath);

            // act
            var acutal = fileStorage.Get("test");

            // assert
            sftpClientMock.Verify(x => x.CreateText($"{baseFilePath}/test"), Times.Once);
        }
    }

    public sealed class DeleteTests
    {
        private readonly Mock<ISftpClient> sftpClientMock = new();

        [Fact(DisplayName = "指定したファイルパスにベースパスを付与してクライアントにリクエストする")]
        public void Test1()
        {
            // arrange
            var baseFilePath = "base";
            var fileStorage = new SFTPFileStorage(sftpClientMock.Object, baseFilePath);

            // act
            fileStorage.Delete("test");

            // assert
            sftpClientMock.Verify(x => x.DeleteDirectory($"{baseFilePath}/test"), Times.Once);
        }

        [Fact(DisplayName = "指定したパスがディレクトリのときディレクトリを削除する")]
        public void Test2()
        {
            // arrange
            var directoryPath = "test";
            var fileStorage = new SFTPFileStorage(sftpClientMock.Object, "");

            // act
            fileStorage.Delete(directoryPath);

            // assert
            sftpClientMock.Verify(x => x.DeleteDirectory(directoryPath), Times.Once);
            sftpClientMock.Verify(x => x.DeleteFile(It.IsAny<string>()), Times.Never);
        }

        [Fact(DisplayName = "指定したパスがファイルのときファイルを削除する")]
        public void Test3()
        {
            // arrange
            var filePath = "test.csv";
            var fileStorage = new SFTPFileStorage(sftpClientMock.Object, "");

            // act
            fileStorage.Delete(filePath);

            // assert
            sftpClientMock.Verify(x => x.DeleteDirectory(It.IsAny<string>()), Times.Never);
            sftpClientMock.Verify(x => x.DeleteFile(filePath), Times.Once);
        }
    }
}
