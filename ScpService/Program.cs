using System;
using System.ServiceProcess;

namespace ScpService 
{
    static class Program 
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main() 
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
