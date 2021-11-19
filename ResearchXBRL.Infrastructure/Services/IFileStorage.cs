using System.Collections.Generic;
using System.IO;

namespace ResearchXBRL.Infrastructure.Services
{
    public interface IFileStorage
    {
        public Stream Get(string filePath);
        public void Set(in Stream inputStream, string filePath);
        IReadOnlyList<string> GetFiles(string directoryPath, string searchPattern = "*");
        public void Unzip(string zipFilePath, string unzippedDirectoryPath);
    }
}
