using System.IO;

namespace ResearchXBRL.Application.DTO
{
    public sealed class EdinetXBRLData
    {
        public string DocumentId { get; init; } = "";
        public Stream ZippedDataStream { get; init; } = new MemoryStream(0);
    }
}
