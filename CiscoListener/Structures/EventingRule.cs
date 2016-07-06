using System.Collections.Generic;

namespace CiscoListener.Structures
{
    public class EventingRule
    {
        public string Condition { get; set; }
        public string Template { get; set; }
        public List<EventingRuleOverride> Overrides { get; set; }
    }
}
