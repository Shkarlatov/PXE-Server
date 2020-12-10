using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PXE_Server
{
    class Utils
    {
        public static string CheckFileInRootDir(string rootDir,string file)
        {
            var tmp = file.Replace('/', '\\');
            if (tmp.StartsWith('\\'))
                tmp = tmp.Substring(1);

            String path = Path.Combine(rootDir, tmp);
            return path;
        }
    }
}
