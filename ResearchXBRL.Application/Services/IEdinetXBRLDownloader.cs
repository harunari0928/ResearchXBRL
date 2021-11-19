using ResearchXBRL.Application.DTO;
using System;
using System.Collections.Generic;

namespace ResearchXBRL.Application.Services
{
    public interface IEdinetXBRLDownloader
    {
        IAsyncEnumerable<EdinetXBRLData> Download(DateTimeOffset start, DateTimeOffset end);
    }
}
