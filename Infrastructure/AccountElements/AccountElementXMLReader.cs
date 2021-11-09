using ResearchXBRL.CrossCuttingInterest.Extensions;
using ResearchXBRL.Domain.AccountElements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace ResearchXBRL.Infrastructure.AccountElements
{
    public sealed class AccountElementXMLReader : IAccountElementReader
    {
        private readonly StreamReader accountItemLabelReader;
        private readonly StreamReader accountItemElementReader;

        public AccountElementXMLReader(string acountItemFilePath, string accountItemLabelPath)
        {
            accountItemLabelReader = new StreamReader(accountItemLabelPath);
            accountItemElementReader = new StreamReader(acountItemFilePath);
        }

        public IEnumerable<AccountElement> Read()
        {
            return CreateAccountElements(
                    accountItemLabelReader,
                    accountItemElementReader);
        }

        private static XmlNode GetAccountItemElement(IEnumerable<XmlNode> accountElements, string elementId)
        {
            return accountElements.Single(x => x.Attributes?["name"]?.Value == elementId);
        }

        private static IEnumerable<(string elementId, string name)> ReadAccountLabels(StreamReader labelReader)
        {
            var accountItemLabelDoc = new XmlDocument();
            accountItemLabelDoc.Load(labelReader);
            var labelLinkContents = ReadLabelLinkContents(accountItemLabelDoc);

            var elementIds = labelLinkContents
                .Where(x => x.Name == "link:loc")
                .Select(x => x.Attributes?["xlink:label"]?.Value ?? throw new Exception("XBRL要素名が空"));
            var names = labelLinkContents
                .Where(x => x.Name == "link:label")
                .GroupBy(x => x.Attributes?["xlink:label"]?.Value.Split('_')[1] ?? "")
                .Select(x => x.First().InnerText ?? throw new Exception("会計項目名が空"));

            return elementIds.Zip(names);
        }

        private static IEnumerable<XmlNode> ReadLabelLinkContents(XmlDocument accountItemLabelDoc)
        {
            return accountItemLabelDoc
                .GetChildNodes()
                .First(x => x.Name == "link:linkbase")
                .GetChildNodes()
                .First(x => x.Name == "link:labelLink")
                .GetChildNodes();
        }

        private static IEnumerable<XmlNode> ReadAccountElements(StreamReader elementReader)
        {
            var accountItemElementDoc = new XmlDocument();
            accountItemElementDoc.Load(elementReader);
            return accountItemElementDoc
                .GetChildNodes()
                .First(x => x.Name == "xsd:schema")
                .GetChildNodes()
                .Where(x => x.Name == "xsd:element");
        }

        private static IEnumerable<AccountElement> CreateAccountElements(StreamReader labelReader, StreamReader elementReader)
        {
            var accountElements = ReadAccountElements(elementReader);
            foreach (var (elementId, name) in ReadAccountLabels(labelReader))
            {
                var accountElement = GetAccountItemElement(accountElements, elementId);
                yield return new AccountElement
                {
                    XBRLName = elementId,
                    AccountName = name,
                    Type = accountElement.Attributes?["type"]?.Value ?? "",
                    SubstitutionGroup = accountElement.Attributes?["substitutionGroup"]?.Value ?? "",
                    Abstract = bool.Parse(accountElement.Attributes?["abstract"]?.Value ?? "false"),
                    Nillable = bool.Parse(accountElement.Attributes?["nillable"]?.Value ?? "false"),
                    Balance = accountElement.Attributes?["xbrli:balance"]?.Value ?? "",
                    PeriodType = accountElement.Attributes?["xbrli:periodType"]?.Value ?? "",
                    TaxonomyVersion = new DateTime(2019, 11, 1)
                };
            }
        }

        public void Dispose()
        {
            foreach (var reader in new TextReader[]
            {
                accountItemLabelReader,
                accountItemElementReader
            })
            {
                reader.Dispose();
            }
        }
    }
}
