using System;
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
        public string handle;   // No use when it's Syn type
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
        public static readonly HoxisProtocolSender nil = new HoxisProtocolSender { id = HoxisID.nil, loopback = true };

        public HoxisID id;
        public bool loopback;
    }
    public struct HoxisProtocolAction
    {
        public static readonly HoxisProtocolAction nil = new HoxisProtocolAction { method = "", args = null };

        public string method;
        public Dictionary<string, string> args;
    }
}
