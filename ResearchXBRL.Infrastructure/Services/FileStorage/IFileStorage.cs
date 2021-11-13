using System.Collections.Generic;

namespace ResearchXBRL.Infrastructure.Services.FileStorage
{
    public interface IFileStorage
    {
        public byte[] Get(string destinationFilePath);
        public void Set(IEnumerable<byte> bytes, string destinationFilePath);
    }
}
