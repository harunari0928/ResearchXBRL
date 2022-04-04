using System;
using System.Collections.Generic;
using System.IO;
using Renci.SshNet;

namespace ResearchXBRL.Infrastructure.Shared.FileStorages;

public sealed class SFTPFileStorage : IFileStorage, IDisposable
{
    private readonly SftpClient client;
    private readonly string baseDirectory;

    public SFTPFileStorage()
    {
        var host = Environment.GetEnvironmentVariable("FILESTORAGE_HOST");
        var user = Environment.GetEnvironmentVariable("FILESTORAGE_USERID");
        var password = Environment.GetEnvironmentVariable("FILESTORAGE_PASSWORD");
        var connectionInfo = new ConnectionInfo(host, user,
            new PasswordAuthenticationMethod(user, password));
        client = new SftpClient(connectionInfo);
        client.Connect();
        baseDirectory = Environment.GetEnvironmentVariable("FILESTORAGE_BASEDIR") ?? "~/";
    }

    public void Delete(string path)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        client.Dispose();
    }

    public Stream? Get(in string filePath)
    {
        var fullFilePath = Path.Combine(baseDirectory, filePath);
        if (!client.Exists(fullFilePath))
        {
            return null;
        }

        return client.OpenRead(fullFilePath);
    }

    public IReadOnlyList<string> GetDirectoryNames(string directoryPath, string searchPattern = "*")
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<string> GetFiles(string directoryPath, string searchPattern = "*")
    {
        throw new NotImplementedException();
    }

    public void Set(in Stream inputStream, in string filePath)
    {
        throw new NotImplementedException();
    }

    public void Unzip(string zipFilePath, string unzippedDirectoryPath)
    {
        throw new NotImplementedException();
    }
}
