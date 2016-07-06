using System;
using System.Linq;
using System.Xml.Linq;

namespace CiscoListener.Helpers
{
    public class Cleaner
    {
        public static string CleanAllNamespaces(string xmlDocument)
        {
            // The XDocument.Parse() method we use elsewhere in the listener
            // adheres strictly to the XML 1.0 specification and in some
            // instances, the Cisco devices will pass encoded illegal characters
            // in hexadecimal format. The underlying XmlTextReaderImpl class
            // will see these and throw an exception at parse time.

            // We will convert these encoded jems into actual representation
            var dirty = InsertTroublesomeCharacters(xmlDocument);

            // Then groom them out if they aren't legal for XML 1.0
            var clean = RemoveTroublesomeCharacters(dirty);

            // And finally remove all namespaces to simplify XPath queries
            return CleanAllNamespaces(XElement.Parse(clean)).ToString();
        }

        public static XElement CleanAllNamespaces(XElement xmlDocument)
        {
            if (!xmlDocument.HasElements)
            {
                var xElement = new XElement(xmlDocument.Name.LocalName)
                {
                    Value = xmlDocument.Value
                };

                foreach (var attribute in xmlDocument.Attributes())
                {
                    xElement.Add(attribute);
                }

                return xElement;
            }
            
            return new XElement(xmlDocument.Name.LocalName, xmlDocument.Elements().Select(CleanAllNamespaces));
        }
        internal static string InsertTroublesomeCharacters(string input)
        {
            // TODO: Look into using Regex to identify and replace these instead.
            // This is limited to hexadecimal encoded char refererences only
            var parts = input.Split(new[] { "&#x" }, StringSplitOptions.None);
            for (var i = 1; i < parts.Length; i++)
            {
                var end = parts[i].IndexOf(';');
                var number = parts[i].Substring(0, end);
                try
                {
                    var unicode = Convert.ToInt32(number, 16);
                    parts[i] = ((char)unicode) + parts[i].Substring(end + 1);
                }
                catch
                {
                    // TODO: If I can't transform it, I should probably kill it
                    // Failure would probably occur at casting or conversion
                    // if the value is outside Int32 or Char limits. Should 
                    // target these with Regex and rip them out completely if
                    // thats the case.
                }
            }
            return string.Join("", parts);
        }

        internal static string RemoveTroublesomeCharacters(string input)
        {
            // There is another char range in the specification, but they
            // aren't valid ranges for the char primitive for .NET, so it's
            // been omitted.
            return new string(input
                .Where(value =>
                    (value >= 0x0020 && value <= 0xD7FF) ||
                    (value >= 0xE000 && value <= 0xFFFD) ||
                    value == 0x9 || // '\t'
                    value == 0xA || // '\n'
                    value == 0xD)   // '\r'
                .ToArray());
        }
    }
}