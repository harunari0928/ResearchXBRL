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
    }
}
