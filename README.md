# PXE Server

PXE Server provides a DHCP server with support PXE and network boot, TFTP server and HTTP server (only send static files).

## Features

Avalible loaders:
- SYSLINUX (bios, efi)
- IPXE (bios, efi)
- SHIM_GRUB2 (with secure boot)
- UEFI_HTTP (not tested on real hardware)

## Todo

- Rewrite hard-coded dhcp boot filename for support user configuration

## Quick start guide

1. Download prepared wwwroot
2. Download minilinux and extract to wwwroot
3. Change loader config:
    - syslinux: pxelinux.cfg\default
    - ipxe: boot.ipxe (it is text script)
    - SHIM GRUB2: grub\grub.cfg
    - EFI HTTP: set url to loader (example: http://192.168.1.100:80/shimx64.efi)
4. Configure PXE (pxe.conf)
5.  Run server
    
## Setup test enviroment

Tested only on Windows 10 x64.

Test EFI x64 boot:
1. Install [TAP-Windows](https://build.openvpn.net/downloads/releases/latest/tap-windows-latest-stable.exe)
2. Install [QEMU](https://qemu.weilnetz.de/w64/)
3. Download [OVMF EDK2](https://retrage.github.io/edk2-nightly/)
4. Run QEMU
> qemu-system-x86_64.exe ^
> -M q35 ^
> -cpu max ^
> -m 512M ^
> -bios RELEASEX64_OVMF.fd ^
> -netdev tap,id=mynet0,ifname=<TAP interface name> -device e1000,netdev=mynet0


Test PXE BIOS boot:
1. Install [VirtualBox](https://www.virtualbox.org/)
2. Create new VM and select network boot.
3. Enjoy!

## If server not work

Check your firewall and open ports:
- UDP: 67, 69
- TCP: 80

On Windows:
> netsh advfirewall firewall add rule name="PXE DHCP" dir=in action=allow protocol=UDP localport=67
> netsh advfirewall firewall add rule name="PXE DHCP" dir=out action=allow protocol=UDP localport=67 
> netsh advfirewall firewall add rule name="PXE TFTP" dir=in action=allow protocol=UDP localport=69
> netsh advfirewall firewall add rule name="PXE TFTP" dir=out action=allow protocol=UDP localport=69 
> netsh advfirewall firewall add rule name="PXE HTTP" dir=in action=allow protocol=TCP localport=80
> netsh advfirewall firewall add rule name="PXE HTTP" dir=out action=allow protocol=TCP localport=80

## Notes

1. How create grub2 pxe loader: 
grub-mkimage -d /usr/lib/grub/i386-pc/ -O i386-pc-pxe -p "(pxe)/grub" -o grub2.pxe pxe tftp pxechain boot http linux

2. How work shim? Shim not work on http.
I use signed shim from ubuntu 20.04. Its pre-compiled with hard-coded filename "grubx64.efi". So I can't set http path. Only work with tftp.

3. Memdisk not work in EFI.
4. Grub loopback can't loop ISO over HTTP :)
5. IPXE sometimes slow downloading over HTTP (maybe bug?)

## List of other open source projects used

- [DHCPServer](https://github.com/jpmikkers/DHCPServer)
- [Tftp.Net](https://github.com/Callisto82/tftp.net)
- [Syslinux](https://wiki.syslinux.org/wiki/index.php?title=The_Syslinux_Project)
- [IPXE](https://ipxe.org/)
- [GRUB2](https://www.gnu.org/software/grub/)
- [UEFI SHIM Loader](https://github.com/rhboot/shim)
- [Minimal Linux](https://github.com/ivandavidov/minimal) and [Minimal Linux Live](http://minimal.idzona.com/#home)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details