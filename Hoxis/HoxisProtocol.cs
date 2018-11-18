using System.Collections;
using System.Collections.Generic;

namespace DacLib.Hoxis
{
    public struct HoxisProtocol
    {
        public static readonly HoxisProtocol nil = new HoxisProtocol { type = "", rcvr = "", method = "", args = null, desc = "" };
        public string type;
        public string rcvr;
        public string method;
        public Dictionary<string, object> args;
        public string desc;
    }
}
