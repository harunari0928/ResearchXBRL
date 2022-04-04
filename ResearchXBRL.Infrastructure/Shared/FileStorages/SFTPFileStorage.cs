using System;
using System.Collections.Generic;
using System.IO;
using Renci.SshNet;

namespace ResearchXBRL.Infrastructure.Shared.FileStorages;

public sealed class SFTPFileStorage : IFileStorage
{
    private readonly ISftpClient client;
    private readonly string baseDirectory;

    public SFTPFileStorage(ISftpClient client, string baseDirectory)
    {
        this.client = client;
        this.baseDirectory = baseDirectory;
    }

    public void Delete(string path)
    {
        throw new NotImplementedException();
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
