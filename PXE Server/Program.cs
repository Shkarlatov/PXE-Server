using System;
using System.IO;

namespace PXE_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = PXEConfig.Load();
            config.Save();

            var pxe_server = new PXEServer(config);
            pxe_server.Start();


            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();

            pxe_server.Stop();

        }
    }
}
