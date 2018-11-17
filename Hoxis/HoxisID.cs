namespace DacLib.Hoxis
{
    public class HoxisID
    {
        public string group { get; }
        public int index { get; }
        public HoxisID(string groupArg, int idArg)
        {
            group = groupArg;
            index = idArg;
        }
    }
}

