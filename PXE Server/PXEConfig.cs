using System;
using System.IO;
using System.Text.Json;


namespace PXE_Server
{
    public class PXEConfig
    {
        public string BindAddress { get; set; } = "192.168.1.27";
        public string NetMask { get; set; } = "255.255.255.0";

        public bool Verbose { get; set; } = true;

        public int DHCPPort { get; set; } = 67;
        public int HTTPPort { get; set; } = 80;
        public int TFTPPort { get; set; } = 69;

        public string ServerDirectory { get; set; } = Path.Combine(Environment.CurrentDirectory, "wwwroot");
        public string Loader { get; set; } = "SYSLINUX";

        public string HTTPBootFile { get; set; } 

        #region save/load
        const string cfg_file_name = "pxe.conf";
        static JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            IgnoreNullValues = true,
            WriteIndented = true
        };


        public static PXEConfig Load()
        {
            try
            {
                var bytes = File.ReadAllBytes(cfg_file_name);
                return JsonSerializer.Deserialize<PXEConfig>(bytes, jsonSerializerOptions);

            }
            catch { return new PXEConfig(); }
        }

        public void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize<PXEConfig>(this, jsonSerializerOptions);
                File.WriteAllText(cfg_file_name, json);
            }
            catch { }
        }

        #endregion


    }

    public enum Loader
    {
        SYSLINUX,
        IPXE,
        SHIM_GRUB2,
        UEFI_HTTP
    }

}
