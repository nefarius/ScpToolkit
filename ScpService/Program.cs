using System;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;
using log4net;

namespace ScpService
{
    internal static class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        private static void Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                try
                {
                    var parameter = string.Concat(args);
                    switch (parameter)
                    {
                        case "--install":
                            Log.Info("Installing Scp Dsx Service");
                            ManagedInstallerClass.InstallHelper(new[] {Assembly.GetExecutingAssembly().Location});
                            Log.Info("Service installed successfully");
                            break;
                        case "--uninstall":
                            Log.Info("Uninstalling Scp Dsx Service");
                            ManagedInstallerClass.InstallHelper(new[] {"/u", Assembly.GetExecutingAssembly().Location});
                            Log.Info("Service uninstalled successfully");
                            break;
                    }
                }
                catch(Exception ex)
                {
                    Log.ErrorFormat("Couldn't (un)install service: {0}", ex);
                }
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new Ds3Service()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}