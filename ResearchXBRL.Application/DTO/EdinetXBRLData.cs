using System;
using System.IO;
using System.Threading.Tasks;

namespace ResearchXBRL.Application.DTO
{
    public sealed class EdinetXBRLData : IAsyncDisposable
    {
        public string DocumentId { get; init; } = "";
        public string DocumentType { get; init; } = "";
        public string CompanyId { get; init; } = "";
        public DateTime DocumentDateTime { get; init; }
        public Lazy<Task<MemoryStream>> LazyZippedDataStream { get; init; } = new();

        public async ValueTask DisposeAsync()
        {
            if (!LazyZippedDataStream.IsValueCreated)
            {
                return;
            }
            var value = await LazyZippedDataStream.Value;
            await value.DisposeAsync();
        }
    }
}
