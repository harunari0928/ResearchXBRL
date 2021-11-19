using System;
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
            if (IsDirectory(filePath))
            {
                throw new IOException($"{nameof(filePath)}には、ファイルパスを指定してください");
            }

            return new FileStream(CreateFullPath(filePath), FileMode.Open);
        }

        public void Set(in Stream inputStream, string filePath)
        {
            var parentPath = Directory.GetParent(CreateFullPath(filePath))?.FullName;

            if (parentPath is null || IsDirectory(filePath))
            {
                throw new IOException($"{nameof(filePath)}には、ファイルパスを指定してください");
            }

            if (!Directory.Exists(parentPath))
            {
                Directory.CreateDirectory(parentPath);
            }

            using var localFileStream = new FileStream(
                CreateFullPath(filePath),
                FileMode.Create);
            inputStream.Position = 0;
            inputStream.CopyTo(localFileStream);
        }

        public IReadOnlyList<string> GetFiles(string directoryPath, string searchPattern = "*")
        {
            if (!IsDirectory(directoryPath))
            {
                throw new IOException($"{nameof(directoryPath)}には、ディレクトリパスを指定してください");
            }

            return Directory
                .GetFiles(CreateFullPath(directoryPath), searchPattern)
                .ToList();
        }

        public void Unzip(string zipFilePath, string unzippedDirectoryPath)
        {
            if (IsDirectory(zipFilePath))
            {
                throw new IOException($"{nameof(zipFilePath)}には、ファイルパスを指定してください");
            }

            if (!IsDirectory(unzippedDirectoryPath))
            {
                throw new IOException($"{nameof(unzippedDirectoryPath)}には、ディレクトリパスを指定してください");
            }

            var tmp = CreateFullPath(zipFilePath);
            ZipFile.ExtractToDirectory(
                CreateFullPath(zipFilePath),
                CreateFullPath(unzippedDirectoryPath));
        }

        private string CreateFullPath(string path)
            => Path.Combine(storageDirectoryBasePath, path);

        private static bool IsDirectory(string path)
        {
            path = path?.Trim() ?? throw new ArgumentNullException(nameof(path));

            if (Directory.Exists(path))
            {
                return true;
            }

            if (File.Exists(path))
            {
                return false;
            }

            if (new[] { "\\", "/" }.Any(path.EndsWith))
            {
                return true;
            }

            return string.IsNullOrWhiteSpace(Path.GetExtension(path));
        }
    }
}
