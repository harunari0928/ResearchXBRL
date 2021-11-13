using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResearchXBRL.Infrastructure.Services.EdinetXBRLDownloaders
{
    public sealed class EdinetXBRLData
    {
        public string DocumentId { get; init; } = "";
        public byte[] BinaryData { get; init; } = Array.Empty<byte>();
    }
}
