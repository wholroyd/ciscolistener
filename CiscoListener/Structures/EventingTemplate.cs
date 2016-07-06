using System.Xml.Serialization;

namespace CiscoListener.Structures
{
    public class EventingTemplate
    {
        [XmlAttribute]
        public bool Default { get; set; }
        [XmlAttribute]
        public string Severity { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        public string Description { get; set; }
        public string Template { get; set; }
        public int EventId { get; set; }
    }
}