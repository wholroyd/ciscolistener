using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace CiscoListener.Helpers
{
    public static class Networking
    {
        public static IPAddress GetAddressForIPv4()
        {
            // For every network interface on the server, give me the first IP address that is...
            // -- not from IPv6
            // -- not a loopback (127.0.0.1)
            // -- not dns ineligible (169.254.x.x)
            // -- not a cluster IP (transient)

            return NetworkInterface.GetAllNetworkInterfaces()
                .Select(x => x.GetIPProperties())
                .SelectMany(properties => properties.UnicastAddresses
                    .Where(x => x.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Where(x => IPAddress.IsLoopback(x.Address) == false)
                    .Where(x => x.IsDnsEligible == true)
                    .Where(x => x.IsTransient == false))
                    .First().Address;
        }

        public static NetTcpBinding GetServiceBindingForNetTcp()
        {
            var binding = new NetTcpBinding { TransactionFlow = false };
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.None;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.None;
            binding.Security.Mode = SecurityMode.None;
            return binding;
        }

        public static NetNamedPipeBinding GetServiceBindingForNetPipe()
        {
            var binding = new NetNamedPipeBinding { TransactionFlow = false };
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.None;
            binding.Security.Mode = NetNamedPipeSecurityMode.None;
            return binding;
        }

        /// <summary>
        /// Use this for Soap12 / http://www.w3.org/2003/05/soap-envelope / Addressing10
        /// </summary>
        /// <returns></returns>
        public static WSHttpBinding GetServiceBindingForHttpWs()
        {
            var binding = new WSHttpBinding { TransactionFlow = false };
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
            binding.Security.Mode = SecurityMode.None;
            binding.MaxReceivedMessageSize = 4194304;
            return binding;
        }

        /// <summary>
        /// Use this for Soap11 / http://schemas.xmlsoap.org/soap/envelope / AddressingNone
        /// </summary>
        /// <returns></returns>
        public static BasicHttpBinding GetServiceBindingForHttpBasic()
        {
            var binding = new BasicHttpBinding();
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
            binding.Security.Mode = BasicHttpSecurityMode.None;
            binding.MaxReceivedMessageSize = 4194304;
            return binding;
        }

        public static WebHttpBinding GetServiceBindingForHttpWeb()
        {
            var binding = new WebHttpBinding();
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
            binding.Security.Mode = WebHttpSecurityMode.None;
            return binding;
        }

        public static CustomBinding AddContentMapper(this Binding model, WebContentTypeMapper mapper)
        {
            var binding = new CustomBinding();

            // Add in the encoding binding
            binding.Elements.Add(new WebMessageEncodingBindingElement() 
            { 
                ContentTypeMapper = mapper 
            });

            // Add in the binding elements from the root binding, skipping the encoding         
            foreach (var item in model.CreateBindingElements()
                .Where(item => !(item is WebMessageEncodingBindingElement))     // We need to keep ours
                .Where(item => !(item is TextMessageEncodingBindingElement)))   // We don't want these (replace with WebMessage)
            {
                binding.Elements.Add(item);
            }

            return binding;
        }
    }
}