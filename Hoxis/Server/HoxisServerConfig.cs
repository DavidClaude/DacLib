using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DacLib.Hoxis.Server
{
    public static class HoxisServerConfig
    {
        public static string basicPath { get { return AppDomain.CurrentDomain.BaseDirectory + "DacLib\\Hoxis"; } }
    }
}
