using System.Collections.Generic;
using System.Xml;

namespace ResearchXBRL.CrossCuttingInterest.Extensions
{
    public static class XmlNodeExtensions
    {
        public static IEnumerable<XmlNode> GetChildNodes(this XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                yield return child;
            }
        }

        public static string? GetAttributeValue(this XmlNode node, string key)
        {
            return node.Attributes?[key]?.Value;
        }
    }
}
