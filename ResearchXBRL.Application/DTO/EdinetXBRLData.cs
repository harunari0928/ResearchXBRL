using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ResearchXBRL.Application.DTO
{
    public sealed class EdinetXBRLData : IAsyncDisposable
    {
        private readonly IEnumerable<IDisposable> disposables;
        public string DocumentId { get; init; } = "";
        public string DocumentType { get; init; } = "";
        public string CompanyId { get; init; } = "";
        public Stream ZippedDataStream { get; init; } = new MemoryStream(0);

        public async ValueTask DisposeAsync()
        {
            await ZippedDataStream.DisposeAsync();
        }
    }
}
