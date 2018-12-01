using System;

namespace DacLib.Hoxis
{
    [Serializable]
    public struct HoxisID
    {
        public static readonly HoxisID undef = new HoxisID("", 0);
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

