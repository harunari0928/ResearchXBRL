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

        public byte[] Get(string fileName)
        {
            using var fileStream = new FileStream(
                Path.Combine(storageDirectoryPath, fileName),
                FileMode.Open);
            using var reader = new BinaryReader(fileStream);
            return ReadBytes(reader).ToArray();
        }

        public void Set(IEnumerable<byte> bytes, string fileName)
        {
            using var fileStream = new FileStream(
                Path.Combine(storageDirectoryPath, fileName),
                FileMode.Create);
            using var writer = new BinaryWriter(fileStream);
            writer.Write(bytes.ToArray());
            writer.Flush();
        }

        private static IEnumerable<byte> ReadBytes(BinaryReader binaryReader)
        {
            while (binaryReader.BaseStream.Position != binaryReader.BaseStream.Length)
            {
                yield return binaryReader.ReadByte();
            }
        }
    }
}
