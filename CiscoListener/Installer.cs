using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Reflection;
using System.ServiceProcess;

namespace CiscoListener
{
    [RunInstaller(true)]
    public class ServiceInstaller : Installer
    {
        private static readonly string Path = Assembly.GetExecutingAssembly().Location;

        // Constructor
        public ServiceInstaller()
        {
            // Define the runtime user
            var process = new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalSystem,
                Username = null,
                Password = null
            };
            Installers.Add(process);

            // Define the service configuration
            var installer = new System.ServiceProcess.ServiceInstaller
            {
                StartType = ServiceStartMode.Automatic,
                ServiceName = "CiscoListener",
                DisplayName = "Cisco Smart Call Home Listener"
            };
            Installers.Add(installer);
        }

        // Self Installation Methods
        public static bool Install()
        {
            // Setup the NT event log source
            if (EventLog.SourceExists("CiscoListener"))
            {
                Console.WriteLine("%% Event log source is already configured.");
            }
            else
            {
                EventLog.CreateEventSource("CiscoListener", "Application");
                Console.WriteLine("%% Event log source is now configured.");
            }

            // Perform service installation
            try
            {
                ManagedInstallerClass.InstallHelper(new[] { Path });
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static bool Uninstall()
        {
            // Remove the NT event log source
            if (EventLog.SourceExists("CiscoListener"))
            {
                EventLog.DeleteEventSource("CiscoListener");
                Console.WriteLine("%% Event log source has been removed.");
            }
            else
            {
                Console.WriteLine("%% Event log source was already removed.");
            }

            // Perform service removal
            try
            {
                ManagedInstallerClass.InstallHelper(new[] { "/u", Path });
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}