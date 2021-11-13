using System;
using System.Collections.Generic;

namespace ResearchXBRL.Infrastructure.Services.EdinetXBRLDownloaders
{
    public interface IEdinetXBRLDownloader
    {
        IAsyncEnumerable<EdinetXBRLData> Download(DateTimeOffset start, DateTimeOffset end);
    }
}
