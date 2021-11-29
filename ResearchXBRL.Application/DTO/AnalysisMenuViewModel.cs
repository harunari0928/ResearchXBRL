using System.Collections.Generic;

namespace ResearchXBRL.Application.DTO
{
    public class AnalysisMenuViewModel
    {
        public IReadOnlyList<string> AccountItems { get; init; } = new string[0];
        public IReadOnlyList<string> Corporations { get; init; } = new string[0];
    }
}
