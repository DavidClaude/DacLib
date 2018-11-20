using System;

namespace DacLib.Hoxis
{
    public struct HoxisID
    {
        public static readonly HoxisID nil = new HoxisID("", 0);
        public static readonly HoxisID server = new HoxisID("server", 0);

        public string group;
        public ushort id;
        public HoxisID(string groupArg, ushort idArg)
        {
            group = groupArg;
            id = idArg;
        }
    }
}

