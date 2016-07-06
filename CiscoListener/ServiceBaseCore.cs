using System;
using System.Diagnostics;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceProcess;

namespace CiscoListener
{
    public abstract class ServiceBaseCore : ServiceBase
    {
        protected ServiceHost Host { get; private set; }
        protected IPAddress ServiceAddress { get; set; }
        protected Binding ServiceBinding { get; set; }
        protected int ServicePort { get; set; }

        protected void GenerateServiceHost(object singleton)
        {
            var address = new Uri($"{ServiceBinding.Scheme}://{ServiceAddress}:{ServicePort}");

            Host = new ServiceHost(singleton, address);
            Host.Opening += OnHostOpening;
            Host.Opened += OnHostOpened;
            Host.Closing += OnHostClosing;
            Host.Closed += OnHostClosed;

            Debug.WriteLine($"%% {ServiceName} host will be reserving {address.AbsoluteUri}{ServiceName}");
        }
        protected void GenerateServiceHost(Type type)
        {
            var address = new Uri($"{ServiceBinding.Scheme}://{ServiceAddress}:{ServicePort}");

            Host = new ServiceHost(type, address);
            Host.Opening += OnHostOpening;
            Host.Opened += OnHostOpened;
            Host.Closing += OnHostClosing;
            Host.Closed += OnHostClosed;

            Debug.WriteLine($"%% {ServiceName} host will be reserving {address.AbsoluteUri}{ServiceName}");
        }

        protected void GenerateServiceEndpoint()
        {
            GenerateServiceEndpoint(Host.SingletonInstance.GetType(), ServiceName, ServiceBinding);
        }

        protected void GenerateServiceEndpoint(Binding binding)
        {
            GenerateServiceEndpoint(Host.SingletonInstance.GetType(), ServiceName, binding);
        }
        protected void GenerateServiceEndpoint(Type type)
        {
            GenerateServiceEndpoint(type, ServiceName, ServiceBinding);
        }
        private void GenerateServiceEndpoint(Type type, string name, Binding binding)
        {
            Host.AddServiceEndpoint(type, binding, name);
            
            var address = new Uri($"{binding.Scheme}://{ServiceAddress}:{ServicePort}");
            Debug.WriteLine($"%% {ServiceName} endpoint is now bound to {address.AbsoluteUri}{ServiceName}");
        }

        private void OnHostClosing(object sender, EventArgs e)
        {
            Debug.WriteLine($"%% {ServiceName} is closing down communications...");
        }
        private void OnHostOpened(object sender, EventArgs e)
        {
            Debug.WriteLine($"%% {ServiceName} is open to communication.");
        }
        private void OnHostClosed(object sender, EventArgs e)
        {
            Debug.WriteLine($"%% {ServiceName} is closed to communication.");
        }
        private void OnHostOpening(object sender, EventArgs e)
        {
            Debug.WriteLine($"%% {ServiceName} is opening communication...");
        }

        public abstract void OnConsoleStart();
        public abstract void OnConsoleStart(int port);

        protected void GenerateMetadataEndpoint()
        {
            // This metadata endpoint allows us to generate a proxy object via 
            // the svcutil.exe tool or "Add Service Reference..." feature in VS

#if (DEBUG)
            var address = new Uri(string.Format("http://{0}:{1}/{2}", ServiceAddress, ServicePort+1, ServiceName));
            var metadata = Host.Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (metadata == null)
            {

                metadata = new ServiceMetadataBehavior
                {
                    HttpGetUrl = address,
                    HttpGetEnabled = true
                };
                Host.Description.Behaviors.Add(metadata);
                Debug.WriteLine($"%% {ServiceName} metadata endpoint is binding to {address.AbsoluteUri}");
            }
            else
            {
                Debug.WriteLine(metadata.HttpGetUrl.ToString());
            }
#endif
        }
    }
}
