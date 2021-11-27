using System.IO;

namespace ResearchXBRL.Application.DTO
{
    public sealed class EdinetTaxonomyData
    {
        public Stream LabelDataStream { get; init; } = new MemoryStream(0);
        public Stream SchemaDataStream { get; init; } = new MemoryStream(0);
    }
}
