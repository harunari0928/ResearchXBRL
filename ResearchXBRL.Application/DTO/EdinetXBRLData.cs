using System;
using System.IO;
using System.Threading.Tasks;
using ResearchXBRL.Application.DTO.Results;

namespace ResearchXBRL.Application.DTO
{
    public sealed class EdinetXBRLData : IAsyncDisposable
    {
        public string DocumentId { get; init; } = "";
        public string DocumentType { get; init; } = "";
        public string CompanyId { get; init; } = "";
        public DateTime DocumentDateTime { get; init; }
        public Lazy<Task<IResult<MemoryStream>>> LazyZippedDataStream { get; init; } = new();

        public async ValueTask DisposeAsync()
        {
            if (!LazyZippedDataStream.IsValueCreated)
            {
                return;
            }
            var value = await LazyZippedDataStream.Value;
            if (value is not Succeeded<MemoryStream> succeeded)
            {
                return;
            }
            await succeeded.Value.DisposeAsync();
        }
    }
}
