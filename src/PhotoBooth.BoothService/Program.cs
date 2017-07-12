using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace PhotoBooth.BoothService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if DEBUG
            PhotoBoothService photoBoothService = new PhotoBoothService();
            photoBoothService.OnDebug();
#else
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new PhotoBoothService() 
            };
            ServiceBase.Run(ServicesToRun);
#endif

        }
    }
}
