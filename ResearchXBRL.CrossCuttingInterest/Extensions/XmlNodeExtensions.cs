using System.Collections.Generic;
using System.Xml;

namespace ResearchXBRL.CrossCuttingInterest.Extensions
{
    public static class XmlNodeExtensions
    {
        public static IEnumerable<XmlNode> GetChildNodes(this XmlNode doc)
        {
            foreach (XmlNode item in doc.ChildNodes)
            {
                yield return item;
            }
        }
    }
}
