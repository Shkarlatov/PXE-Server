using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

using DHCP = GitHub.JPMikkers.DHCP;

namespace PXE_Server
{

    public class PXEServer : IDisposable
    {
        public IPAddress BindAddress { get; set; }
        public IPAddress NetMask { get; set; }

        public int DHCPPort { get; set; }
        public int HTTPPort { get; set; }
        public int TFTPPort { get; set; }

        public string ServerDirectory { get; set; }

        private TFTPServer tftp_server;
        private DHCPServer dhcp_server;
        private HttpFileServer http_server;

        private Loader loader;


        public PXEServer(PXEConfig config)
        {
            BindAddress = IPAddress.Parse(config.BindAddress);
            NetMask = IPAddress.Parse(config.NetMask);

            DHCPPort = config.DHCPPort;
            HTTPPort = config.HTTPPort;
            TFTPPort = config.TFTPPort;

            ServerDirectory = config.ServerDirectory;
            loader = Enum.Parse<Loader>(config.Loader);
            if(config.Verbose)
            {
                Trace.Listeners.Add(new ConsoleTraceListener());
            }
           

        }
        public PXEServer()
        {
            DHCPPort = 67;
            HTTPPort = 80;
            TFTPPort = 69;
            ServerDirectory = Path.Combine(Environment.CurrentDirectory, "wwwroot");

            BindAddress = IPAddress.Parse("192.168.1.27");
            NetMask = IPAddress.Parse("255.255.255.0");

         
        }
        public void Start()
        {

            Stop();

            http_server = new HttpFileServer(HTTPPort, ServerDirectory);
            http_server.Start();

            tftp_server = new TFTPServer(BindAddress, TFTPPort, ServerDirectory);

            var net = new IPSegment(BindAddress.ToString(), NetMask.ToString());

            dhcp_server = new DHCPServer(BindAddress, DHCPPort);
            dhcp_server.Loader = loader;
            dhcp_server.SubnetMask = IPAddress.Parse("255.255.255.0");
            dhcp_server.PoolStart = net.Hosts().First().ToIpAddress();
            dhcp_server.PoolEnd = net.Hosts().Last().ToIpAddress();
            dhcp_server.Start();
        }
        public void Stop()
        {
            tftp_server?.Dispose();

            dhcp_server?.Dispose();
            
            http_server?.Dispose();
        }


        #region dispose
        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Stop();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~PXEServer()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
