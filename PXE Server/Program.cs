using System;
using System.IO;

namespace PXE_Server
{
    class Program
    {
        static void Main(string[] args)
        {

            var server = new HttpFileServer();
            server.Start();
            Console.WriteLine("Hello World!");
            Console.ReadLine();
            server.Stop();
        }
    }
}
