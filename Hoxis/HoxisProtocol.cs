using System.Collections;
using System.Collections.Generic;

namespace DacLib.Hoxis
{
    public struct HoxisProtocol
    {
        public static readonly HoxisProtocol nil = new HoxisProtocol
        {
            type = ProtocolType.None,
            rcvr = HoxisProtocolReceiver.nil,
            sndr = HoxisProtocolSender.nil,
            action = HoxisProtocolAction.nil,
            desc = ""
        };
        public ProtocolType type;
        public HoxisProtocolReceiver rcvr;
        public HoxisProtocolSender sndr;
        public HoxisProtocolAction action;
        public string desc;
    }
    public struct HoxisProtocolReceiver
    {
        public static readonly HoxisProtocolReceiver nil = new HoxisProtocolReceiver { type = ReceiverType.None, id = HoxisID.nil };
        public ReceiverType type;
        public HoxisID id;
    }
    public struct HoxisProtocolSender
    {
        public static readonly HoxisProtocolSender nil = new HoxisProtocolSender { id = HoxisID.nil, back = true };
        public HoxisID id;
        public bool back;
    }
    public struct HoxisProtocolAction
    {
        public static readonly HoxisProtocolAction nil = new HoxisProtocolAction { mthd = "", args = null };
        public string mthd;
        public Dictionary<string, object> args;
    }
}
