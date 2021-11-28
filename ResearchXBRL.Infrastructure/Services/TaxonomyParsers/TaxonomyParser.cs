using ResearchXBRL.Application.DTO;
using ResearchXBRL.Application.Services;
using ResearchXBRL.CrossCuttingInterest.Extensions;
using ResearchXBRL.Domain.AccountElements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace ResearchXBRL.Infrastructure.Services.TaxonomyParsers
{
    public sealed class AccountElementXMLReader : ITaxonomyParser
    {
        public IEnumerable<AccountElement> Parse(EdinetTaxonomyData source)
        {
            return CreateAccountElements(source);
        }

        private static XmlNode GetAccountItemElement(IEnumerable<XmlNode> accountElements, string elementId)
        {
            return accountElements.Single(x => x.GetAttributeValue("name") == elementId);
        }

        private static IEnumerable<(string elementId, string name)> ReadAccountLabels(TextReader labelReader)
        {
            var labelLinkContents = ReadLabelLinkContents(labelReader);

            var elementIds = labelLinkContents
                .Where(x => x.Name == "link:loc")
                .Select(x => x.GetAttributeValue("xlink:label") ?? throw new Exception("XBRL要素名が空"));
            var names = labelLinkContents
                .Where(x => x.Name == "link:label")
                .GroupBy(x => x?.GetAttributeValue("xlink:label")?.Split('_')[1] ?? "")
                .Select(x => x.First().InnerText ?? throw new Exception("会計項目名が空"));

            return elementIds.Zip(names);
        }

        private static IEnumerable<XmlNode> ReadLabelLinkContents(TextReader labelReader)
        {
            var accountItemLabelDoc = new XmlDocument();
            accountItemLabelDoc.Load(labelReader);
            return accountItemLabelDoc
                .GetChildNodes()
                .First(x => x.Name == "link:linkbase")
                .GetChildNodes()
                .First(x => x.Name == "link:labelLink")
                .GetChildNodes();
        }

        private static IEnumerable<XmlNode> ReadAccountElements(TextReader schemaReader)
        {
            var accountItemSchemaDoc = new XmlDocument();
            accountItemSchemaDoc.Load(schemaReader);
            return accountItemSchemaDoc
                .GetChildNodes()
                .First(x => x.Name == "xsd:schema")
                .GetChildNodes()
                .Where(x => x.Name == "xsd:element");
        }

        private static IEnumerable<AccountElement> CreateAccountElements(EdinetTaxonomyData source)
        {
            using var labelReader = new StreamReader(source.LabelDataStream);
            using var schemaReader = new StreamReader(source.SchemaDataStream);
            var accountElements = ReadAccountElements(schemaReader);
            foreach (var (elementId, name) in ReadAccountLabels(labelReader))
            {
                var accountElement = GetAccountItemElement(accountElements, elementId);
                yield return new AccountElement
                {
                    XBRLName = elementId,
                    AccountName = name,
                    Type = accountElement.GetAttributeValue("type") ?? "",
                    SubstitutionGroup = accountElement.GetAttributeValue("substitutionGroup") ?? "",
                    Abstract = bool.Parse(accountElement.GetAttributeValue("abstract") ?? "false"),
                    Nillable = bool.Parse(accountElement.GetAttributeValue("nillable") ?? "false"),
                    Balance = accountElement.GetAttributeValue("xbrli:balance") ?? "",
                    PeriodType = accountElement.GetAttributeValue("xbrli:periodType") ?? "",
                    TaxonomyVersion = source.TaxonomyVersion,
                    Classification = source.Classification
                };
            }
        }
    }
}
