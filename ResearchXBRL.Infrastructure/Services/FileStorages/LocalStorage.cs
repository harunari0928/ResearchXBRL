using ResearchXBRL.Application.Services;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace ResearchXBRL.Infrastructure.Services.FileStorages
{
    public sealed class LocalStorage : IFileStorage
    {
        private readonly string storageDirectoryBasePath;

        public LocalStorage(string storageDirectoryBasePath)
        {
            if (!Directory.Exists(storageDirectoryBasePath))
            {
                Directory.CreateDirectory(storageDirectoryBasePath);
            }
            this.storageDirectoryBasePath = storageDirectoryBasePath;
        }

        public Stream Get(string filePath)
        {
            return new FileStream(CreateFullPath(filePath), FileMode.Open);
        }

        public void Set(in Stream inputStream, string filePath)
        {
            using var localFileStream = new FileStream(
                CreateFullPath(filePath),
                FileMode.Create);
            using var writer = new StreamWriter(localFileStream);
            writer.Write(inputStream);
            writer.Flush();
        }

        public IReadOnlyList<string> GetFiles(string directoryPath, string searchPattern = "*")
        {
            return Directory.GetFiles(directoryPath, searchPattern).ToList();
        }

        public void Unzip(string zipFilePath, string unzippedDirectoryPath, bool isDeleteOriginalZipFile)
        {
            ZipFile.ExtractToDirectory(
                CreateFullPath(zipFilePath),
                CreateFullPath(unzippedDirectoryPath));
            if (isDeleteOriginalZipFile)
            {
                File.Delete(CreateFullPath(zipFilePath));
            }
        }

        private string CreateFullPath(string path)
            => Path.Combine(storageDirectoryBasePath, path);
    }
}
