# PXE Server

DHCP Server + TFTP Server + HTTP Server + PXELINUX

## HTTP Server

HTTP Server is very simple. He can only send static files.

##  DHCP Server not work

If your DHCP server is running on a real network you should add a firewall rule to allow DHCP UDP traffic. This can be done by running the following commands as admin:

> netsh advfirewall firewall add rule name="DotNetDHCPServer" dir=in action=allow protocol=UDP localport=67
> netsh advfirewall firewall add rule name="DotNetDHCPServer" dir=out action=allow protocol=UDP localport=67 

## List of other open source libraries used

- [DHCPServer](https://github.com/jpmikkers/DHCPServer)
- [Tftp.Net](https://github.com/Callisto82/tftp.net)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details