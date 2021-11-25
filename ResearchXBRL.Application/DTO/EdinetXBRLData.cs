using System.IO;

namespace ResearchXBRL.Application.DTO
{
    public sealed class EdinetXBRLData
    {
        public string DocumentId { get; init; } = "";
        public string DocumentType { get; init; } = "";
        public string CompanyId { get; init; } = "";
        public Stream ZippedDataStream { get; init; } = new MemoryStream(0);
    }
}
