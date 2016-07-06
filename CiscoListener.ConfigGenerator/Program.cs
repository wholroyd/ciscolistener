using System;
using CiscoListener.Helpers;
using CiscoListener.Structures;

namespace CiscoListener.ConfigGenerator
{
    internal class Program
    {
        private static void Main(string[] args)
        {

            var routing = new EventingConfiguration
            {
                Rules = new EventingRuleCollection
                {
                    new EventingRule()
                    {
                        Condition = "//Header[Severity<5]/Severity/text()",
                        Template = "Warning",
                    }
                },
                Templates = new EventingTemplateCollection
                {
                    new EventingTemplate
                    {
                        Name = "Warning",
                        Severity = "Warning",
                        EventId = 1001,
                        Description = "Opens low priority ticket",
                        Template = @"At approximately $//CallHome/EventTime$, the following device sent a message:

Make/Model:      $//CallHome/Event/Brand$ $//CallHome/Event/Series$
Device Name:     $//CallHome//SystemInfo/Name$
Chassis Model:   $//CallHome//Chassis/Model$
Chassis Version: $//CallHome//Chassis/HardwareVersion$
Chassis Serial:  $//CallHome//Chassis/SerialNumber$

The message sent was:

$//CallHome/MessageDescription$"

                    },
                    new EventingTemplate
                    {
                        Name = "Error",
                        Default = true,
                        Severity = "Error",
                        EventId = 1002,
                        Description = "Opens medium priority ticket",
                        Template = @"At approximately $//CallHome/EventTime$, the following device sent a message:

Make/Model:      $//CallHome/Event/Brand$ $//CallHome/Event/Series$
Device Name:     $//CallHome//SystemInfo/Name$
Chassis Model:   $//CallHome//Chassis/Model$
Chassis Version: $//CallHome//Chassis/HardwareVersion$
Chassis Serial:  $//CallHome//Chassis/SerialNumber$

The message sent was:

$//CallHome/MessageDescription$"

                    }
                }
            };

            try
            {
                const string configurationXml = "configuration.xml";
                const string configurationJson = "configuration.json";

                Serializer.SaveXml(configurationXml, routing);
                Serializer.SaveJson(configurationJson, routing);
                Console.WriteLine("## Configuration written successfully.");

                Serializer.LoadXml(configurationXml, out routing);
                Serializer.LoadJson(configurationJson, out routing);
                Console.WriteLine(
                    $"## Configuration consumed successfully.\n-- {routing.Rules.Count} rules loaded\n-- {routing.Templates.Count} templates loaded");

            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"## Configuration write failed due to {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            }

            Console.Read();
        }
    }
}
