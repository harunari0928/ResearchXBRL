using System;
using System.Collections.Generic;

namespace ResearchXBRL.Infrastructure.Services.EdinetXBRLDownloader
{
    public interface IEdinetXBRLDownloader
    {
        IAsyncEnumerable<EdinetXBRLData> Download(DateTimeOffset start, DateTimeOffset end);
    }
}
