using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CiscoListener.Helpers;
using CiscoListener.Interfaces;
using CiscoListener.Structures;

namespace CiscoListener
{
    [ServiceBehavior(InstanceContextMode =
        InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple,
        AddressFilterMode = AddressFilterMode.Any,
        ValidateMustUnderstand = false)]
    public class Listener : IListener
    {
        private const string Path = "configuration.xml";
        private static EventingConfiguration _configuration;
        private readonly FileSystemWatcher _watcher;

        public Listener()
        {
            // Populate configuration from disk
            Serializer.LoadXml(Path, out _configuration);

            // Configure file watcher to pick up changes
            _watcher = new FileSystemWatcher(Environment.CurrentDirectory)
            {
                Filter = Path,
                NotifyFilter = NotifyFilters.LastWrite |
                                NotifyFilters.CreationTime |
                                NotifyFilters.Size
            };

            // The FileSystemWatcher can raise multiple events for a single change to the configuration file.
            // So to prevent this, we temporarily disable EnableRaisingEvents until we're done.

            // On change
            _watcher.Changed += (s, e) =>
            {
                _watcher.EnableRaisingEvents = false;
                Debug.WriteLine("%% Eventing configuration was changed on disk.");
                UpdateConfiguration();
                _watcher.EnableRaisingEvents = true;
            };

            // On created
            _watcher.Created += (s, e) =>
            {
                _watcher.EnableRaisingEvents = false;
                Debug.WriteLine("%% Eventing configuration was created.");
                UpdateConfiguration();
                _watcher.EnableRaisingEvents = true;
            };

            // On deleted
            _watcher.Deleted += (s, e) =>
            {
                _watcher.EnableRaisingEvents = false; 
                Debug.WriteLine("%% Eventing configuration was deleted from disk; existing configuration in memory will remain.");
                Debug.WriteLine("%% Eventing configuration should be replaced as soon as possible or service will fail on startup.");
                _watcher.EnableRaisingEvents = true;
            };

            // Begin watching
            _watcher.EnableRaisingEvents = true;
        }

        private static void UpdateConfiguration()
        {
            try
            {
                EventingConfiguration test;
                Serializer.LoadXml(Path, out test);

                // Thread safety
                lock (_configuration)
                {
                    _configuration = test;
                }

                Debug.WriteLine("%% Eventing configuration was updated in memory.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("%% Eventing configuration failed to load; existing configuration in memory will remain.");
                Debug.WriteLine($"%% Eventing configuration update failure occured due to: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public Message ProcessMessage(Message message)
        {
            Debug.WriteLine("\nNotification received:");

            try
            {
                // Once this message has been read and consumed, it can no longer be consumed again. So let's
                // create a buffered copy of the message in memory so we can read it and make copies as needed
                var buffer = message.CreateBufferedCopy(int.MaxValue);
                var body = ProcessMessage(buffer);

                // Evaluate which template should be applied based on rules
                foreach (var rule in _configuration.Rules.Where(rule => body.XPathSelectElement(rule.Condition) != null))
                {
                    ProcessMessage(_configuration.Templates.Single(t => t.Name == rule.Template), body);
                    return buffer.CreateMessage();
                }

                // Use the default template
                ProcessMessage(_configuration.Templates.Single(t => t.Default), body);
                return buffer.CreateMessage();

            }
            catch (XmlException ex)
            {
                // We don't want to put an unprintable character into the EventLog or things like SCOM will lose its mind.
                // So lets be nice and skip around the character in the exception details.
                var exception =  ex.Message.Contains("is an invalid character")
                    ? ex.ToString().Substring(0, 25) + ex.ToString().Substring(30) // TODO: this should be a regex
                    : ex.ToString();

                EventLog.WriteEntry("CiscoListener", exception, EventLogEntryType.Warning, 3);
                Debug.WriteLine($"\nEncountered an exception while processing the message...\n{ex}");

                throw;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("CiscoListener", ex.ToString(), EventLogEntryType.Warning, 3);
                Debug.WriteLine($"\nEncountered an exception while processing the message...\n{ex}");

                throw;
            }
        }

        private static XElement ProcessMessage(MessageBuffer buffer)
        {
            // Transform the namespace marked XML into normalized XML.
            var outer = buffer.CreateMessage().GetReaderAtBodyContents().ReadOuterXml();
            var clean = Cleaner.CleanAllNamespaces(outer);
            return XElement.Parse(clean);
        }

        private static void ProcessMessage(EventingTemplate template, XNode body)
        {
            // Do AdLib on template with values from message
            var output = template.Template;
            foreach (Match match in Regex.Matches(output, @"\$(.*?)\$", RegexOptions.CultureInvariant))
            {
                var token = match.Groups[1].Value;
                var value = body.XPathSelectElement(token).Value;
                output = output.Replace(match.Groups[0].Value, value.Trim());
            }

            Debug.WriteLine($"Using template: {template.Name}");
            Debug.WriteLine($"{"-".PadRight(40, '-')}\n{output}\n{"-".PadRight(40, '-')}");

            var severity = (EventLogEntryType) Enum.Parse(typeof (EventLogEntryType), template.Severity);

            EventLog.WriteEntry("CiscoListener", output, severity, template.EventId);
        }
    }
}
