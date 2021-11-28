using System;
using System.Collections.Generic;
using System.IO;

namespace ResearchXBRL.Application.DTO
{
    public sealed class EdinetXBRLData : IDisposable
    {
        private readonly IEnumerable<IDisposable> disposables;
        public string DocumentId { get; init; } = "";
        public string DocumentType { get; init; } = "";
        public string CompanyId { get; init; } = "";
        public Stream ZippedDataStream { get; init; } = new MemoryStream(0);

        public EdinetXBRLData() { }
        public EdinetXBRLData(IEnumerable<IDisposable> disposables)
        {
            this.disposables = disposables;
        }

        public void Dispose()
        {
            ZippedDataStream.Dispose();
            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }
        }
    }
}
