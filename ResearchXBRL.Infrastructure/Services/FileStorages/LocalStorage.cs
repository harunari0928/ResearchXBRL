using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ResearchXBRL.Infrastructure.Services.FileStorages
{
    public sealed class LocalStorage : IFileStorage
    {
        private readonly string storageDirectoryPath;

        public LocalStorage(string storageDirectoryPath)
        {
            if (!Directory.Exists(storageDirectoryPath))
            {
                Directory.CreateDirectory(storageDirectoryPath);
            }
            this.storageDirectoryPath = storageDirectoryPath;
        }

        public Stream Get(string filePath)
        {
            return new FileStream(
                Path.Combine(storageDirectoryPath, filePath),
                FileMode.Open);
        }

        public void Set(IEnumerable<byte> bytes, string filePath)
        {
            using var fileStream = new FileStream(
                Path.Combine(storageDirectoryPath, filePath),
                FileMode.Create);
            using var writer = new BinaryWriter(fileStream);
            writer.Write(bytes.ToArray());
            writer.Flush();
        }

        public void Unzip()
        {
            throw new System.NotImplementedException();
        }
    }
}
