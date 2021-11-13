using System;

namespace ResearchXBRL.Infrastructure.Services.EdinetXBRLDownloaders
{
    public sealed class EdinetXBRLData
    {
        public string DocumentId { get; init; } = "";
        public byte[] BinaryData { get; init; } = Array.Empty<byte>();
    }
}
