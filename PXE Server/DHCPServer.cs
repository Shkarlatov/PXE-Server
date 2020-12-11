using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;

using GitHub.JPMikkers.DHCP;

using DHCP = GitHub.JPMikkers.DHCP;

namespace PXE_Server
{

    public class DHCPServer : DHCP.DHCPServer
    {
        public IPAddress BindAddress { get; set; } = IPAddress.Parse("192.168.1.27");
        public Loader Loader { get; set; } = Loader.SYSLINUX;
        public string HTTPBootFile {get;set;}

        public DHCPServer(IPAddress address) : this(address, 67) { }
        public DHCPServer(IPAddress address, int port) : base(null)
        {
            BindAddress = address;

            this.EndPoint = new IPEndPoint(BindAddress, port); // default port
            this.SubnetMask = IPAddress.Parse("255.255.255.0");
            this.PoolStart = IPAddress.Parse("192.168.1.100");
            this.PoolEnd = IPAddress.Parse("192.168.1.200");
            this.LeaseTime = DHCP.Utils.InfiniteTimeSpan;
            this.OfferExpirationTime = TimeSpan.FromSeconds(30);

            this.MinimumPacketSize = 576;


            this.OnStatusChange += Dhcpd_OnStatusChange;
            this.OnTrace += Dhcpd_OnTrace;

            Options.Add(new DHCP.OptionItem(mode: DHCP.OptionMode.Force,
                option: new DHCP.DHCPOptionRouter()
                {
                    IPAddresses = new[] { BindAddress }
                }));

            Options.Add(new DHCP.OptionItem(mode: DHCP.OptionMode.Force,
               option: new DHCP.DHCPOptionServerIdentifier(BindAddress)));

            Options.Add(new DHCP.OptionItem(mode: DHCP.OptionMode.Force,
              option: new DHCP.DHCPOptionTFTPServerName(Dns.GetHostName())));

            Options.Add(new DHCP.OptionItem(mode: DHCP.OptionMode.Force,
                 option: new DHCP.DHCPOptionHostName(Dns.GetHostName())));

        }

        // https://www.ietf.org/assignments/dhcpv6-parameters/dhcpv6-parameters.xml#processor-architecture
        private readonly Dictionary<(Loader, byte), string> avalibleArch = new Dictionary<(Loader, byte), string>()
        {
            { (Loader.SYSLINUX,00),"lpxelinux.0" },
            { (Loader.SYSLINUX,06),"syslinux32.efi" },
            { (Loader.SYSLINUX,07),"syslinux64.efi" },


            { (Loader.IPXE,00),"ipxe.pxe" },
            { (Loader.IPXE,07),"ipxe.efi" },

            { (Loader.SHIM_GRUB2,00),"grub2.pxe" },
            { (Loader.SHIM_GRUB2,07),"shimx64.efi" },

            { (Loader.UEFI_HTTP,07),"shimx64.efi" }, 

        };
        protected override void ProcessingReceiveMessage(DHCPMessage sourceMsg, DHCPMessage targetMsg)
        {
            var bootFile = string.Empty;


            if (sourceMsg.isHTTP())
            {
                bootFile = HTTPBootFile;
            }
            else
            if (sourceMsg.isIPXE())
            {
                // this is ipxe script
                // bootFile = "http://192.168.1.27:8080/boot.ipxe";
                bootFile = HTTPBootFile;
            }
            else


            if (sourceMsg.isPXE())
            {
                var arch = sourceMsg.GetArch();
                bootFile = avalibleArch[(Loader,arch)];
            }


            targetMsg.BootFileName = bootFile;
            targetMsg.NextServerIPAddress = BindAddress;
        }

        public new void Start()
        {
            base.Start();
        }


        private void Dhcpd_OnTrace(object sender, DHCP.DHCPTraceEventArgs e)
        {
            Trace.WriteLine(e?.Message);
            Trace.Flush();
        }

        private void Dhcpd_OnStatusChange(object sender, DHCP.DHCPStopEventArgs e)
        {
            Trace.WriteLine(e?.Reason);
            Trace.Flush();
        }
    }


    public static class DHCPMessageExtensions
    {
        public static byte GetArch(this DHCPMessage message)
        {
            try
            {
                return message.Options
             .Where(x => x.OptionType == TDHCPOption.ClientSystemArchitectureType)
             .Cast<DHCPOptionGeneric>()
             .Select(x => x.Data[1])
             .First();
            }
            catch { return 0; }
        }

        public static string GetVendorClass(this DHCPMessage message)
        {
            var sb = new StringBuilder();
            var strings = message.Options
             .Where(x => x.OptionType == TDHCPOption.VendorClassIdentifier)
             .Cast<DHCPOptionVendorClassIdentifier>()
             .Select(x => Encoding.ASCII.GetString(x.Data));

            try
            {
                foreach (var s in strings)
                {
                    sb.AppendLine(s);
                }
            }
            catch { }
            return sb.ToString();
        }
        public static bool isHTTP(this DHCPMessage message) => GetVendorClass(message).Contains("HTTPClient");
        public static bool isPXE(this DHCPMessage message) => GetVendorClass(message).Contains("PXEClient");
        public static bool isIPXE(this DHCPMessage message)
        {
            try
            {
                return message.Options
                    .Where(x => x.OptionType == (TDHCPOption)77)
                    .Cast<DHCPOptionGeneric>()
                    .Select(x => Encoding.ASCII.GetString(x.Data))
                    .Any(x => x.Equals("iPXE", StringComparison.InvariantCultureIgnoreCase));
            }
            catch { return false; }
        }

    }
}
