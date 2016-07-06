using System;
using System.Diagnostics;
using CiscoListener.Helpers;
using CiscoListener.Interfaces;
using CiscoListener.Properties;

namespace CiscoListener
{
    public partial class ListenerHost : ServiceBaseCore
    {    
        public ListenerHost()
        {
            InitializeComponent();

            ServiceName = typeof(Listener).Name;
            ServicePort = int.Parse(Settings.Default["listener_port"].ToString());
            ServiceAddress = Networking.GetAddressForIPv4();
            ServiceBinding = Networking.GetServiceBindingForHttpWs()
                .AddContentMapper(new ListenerHostWebContentTypeMapper());
        }

        protected override void OnStart(string[] args)
        {
            Debug.WriteLine($"%% {ServiceName} is starting...");

            try
            {
                GenerateServiceHost(typeof(Listener));
                GenerateServiceEndpoint(typeof(IListener));
                GenerateMetadataEndpoint();
                Host.Open();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }       
        }

        protected override void OnStop()
        {
            Debug.WriteLine($"%% {ServiceName} is stopping...");
            Host.Close();
        }

        // Debug entry (for console usage)
        public override void OnConsoleStart()
        {
            OnConsoleStart(ServicePort);
        }
        public override void OnConsoleStart(int port)
        {
            ServicePort = port;

            OnStart(null);
        }
    }
}