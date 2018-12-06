using System;

namespace DacLib.Hoxis
{
    [Serializable]
    public struct HoxisAgentID
    {
        public static readonly HoxisAgentID undef = new HoxisAgentID("", 0);
        public static readonly HoxisAgentID server = new HoxisAgentID("server", 0);

        public string group;
        public ushort id;
        public HoxisAgentID(string groupArg, ushort idArg)
        {
            group = groupArg;
            id = idArg;
        }
    }
}

