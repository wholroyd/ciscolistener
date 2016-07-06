using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using ServiceInstaller = CiscoListener.ServiceInstaller;

namespace CiscoListener
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main(string[] args)
        {
            if (args != null && args.Length == 1 && args[0].Length > 1 && (args[0][0] == '-' || args[0][0] == '/'))
            {
                // Link the debug output to console output
                Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
                Console.Clear();

                var self = Assembly.GetExecutingAssembly();
                var attributes = self.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);

                // Welcome to CiscoListener
                var welcome = string.Format(CultureInfo.CurrentCulture, "{0} v{1}", self.GetName().Name, self.GetName().Version);
                Console.Title = welcome + " (Console Mode)";
                Console.WriteLine("\n{0}", welcome);
                Console.WriteLine("{0}\n", ((AssemblyDescriptionAttribute)attributes[0]).Description.ToString(CultureInfo.CurrentCulture));

                Console.WriteLine("%% Debug sink attached to console\n");

                switch (args[0].Substring(1).ToLower())
                {                   
                    case "install":
                    case "i":
                        ServiceInstaller.Install();
                        break;
                    case "uninstall":
                    case "u":
                        ServiceInstaller.Uninstall();
                        break;
                    case "console":
                    case "c":
                        var service = new ListenerHost();
                        service.OnConsoleStart();
            
                        Thread.Sleep(Timeout.Infinite);

                        break;
                }
            }
            else
            {
                var services = new ServiceBase[] 
                { 
                    new ListenerHost()
                };
                ServiceBase.Run(services);
            }
        }     
    }
}