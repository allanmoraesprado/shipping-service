using System.ServiceProcess;

namespace SinoTotalExpress
{
    static class Program
    {
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Integrador()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
