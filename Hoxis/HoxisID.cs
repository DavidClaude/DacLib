namespace DacLib.Hoxis
{
    public struct HoxisID
    {
        public static readonly HoxisID nil = new HoxisID("", -1);

        public string group { get; }
        public int index { get; }
        public HoxisID(string groupArg, int idArg)
        {
            group = groupArg;
            index = idArg;
        }
    }
}

