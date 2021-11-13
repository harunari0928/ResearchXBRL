using System.Collections.Generic;
using System.IO;

namespace ResearchXBRL.Infrastructure.Services
{
    public interface IFileStorage
    {
        public Stream Get(string filePath);
        public void Set(IEnumerable<byte> bytes, string filePath);
        public void Unzip();
    }
}
