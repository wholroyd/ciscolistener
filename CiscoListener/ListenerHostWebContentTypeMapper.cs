using System.ServiceModel.Channels;

namespace CiscoListener
{
    /// <summary>
    /// Cisco devices configured to use the XML format will communicate by sending non-conformant SOAP 1.2 
    /// bodies with non-compliant content-types in the header. As a result, WCF isn't able to determine
    /// the proper XML-based protocol being used and will throw a System.ServiceModel.ProtocolException. 
    /// </summary>
    public class ListenerHostWebContentTypeMapper : WebContentTypeMapper
    {
        public override WebContentFormat GetMessageFormatForContentType(string contentType)
        {
            return contentType.StartsWith("application/x-www-form-urlencoded", System.StringComparison.CurrentCultureIgnoreCase)
                ? WebContentFormat.Xml 
                : WebContentFormat.Default;
        }
    }
}