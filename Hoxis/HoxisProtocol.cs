using System;
using System.Collections.Generic;
using DacLib.Generic;

namespace DacLib.Hoxis
{
    public struct HoxisProtocol
    {
        /// <summary>
        /// Default and invalid HoxisProtocol
        /// </summary>
        public static readonly HoxisProtocol undef = new HoxisProtocol
        {
            type = ProtocolType.None,
            handle = "",
            err = false,
            receiver = HoxisProtocolReceiver.undef,
            sender = HoxisProtocolSender.undef,
            action = HoxisProtocolAction.undef,
            desc = ""
        };

        /// <summary>
        /// Which intention of this protocol, synchronization, request or response ?
        /// </summary>
        public ProtocolType type;

        /// <summary>
        /// The key to match one request and response, no use for synchronization
        /// </summary>
        public string handle;

        /// <summary>
        /// The flag if response succeeds, no use except response
        /// </summary>
        public bool err;

        /// <summary>
        /// Whom will receive this protocol
        /// </summary>
        public HoxisProtocolReceiver receiver;

        /// <summary>
        /// Who sends this protocol
        /// </summary>
        public HoxisProtocolSender sender;

        /// <summary>
        /// What to do after reception
        /// </summary>
        public HoxisProtocolAction action;

        /// <summary>
        /// What information has to be emphasized ?
        /// </summary>
        public string desc;
    }
    public struct HoxisProtocolReceiver
    {
        public static readonly HoxisProtocolReceiver undef = new HoxisProtocolReceiver { type = ReceiverType.None, hid = HoxisID.undef };

        /// <summary>
        /// Server, cluster of current game, teammates, or some player ?
        /// </summary>
        public ReceiverType type;
        
        /// <summary>
        /// If to some player, what's his HoxisID
        /// </summary>
        public HoxisID hid;

        public HoxisProtocolReceiver(ReceiverType typeArg, HoxisID id)
        {
            type = typeArg;
            hid = id;
        }
    }
    public struct HoxisProtocolSender
    {
        public static readonly HoxisProtocolSender undef = new HoxisProtocolSender { hid = HoxisID.undef, loopback = true };

        /// <summary>
        /// HoxisID of sender
        /// </summary>
        public HoxisID hid;

        /// <summary>
        /// Should server return this protocol when broadcasting ?
        /// </summary>
        public bool loopback;

        public HoxisProtocolSender(HoxisID id, bool loopbackArg = true)
        {
            hid = id;
            loopback = loopbackArg;
        }
    }
    public struct HoxisProtocolAction
    {
        public static readonly HoxisProtocolAction undef = new HoxisProtocolAction { method = "", args = HoxisProtocolArgs.undef };

        /// <summary>
        /// Method name in actionTable
        /// </summary>
        public string method;

        /// <summary>
        /// Arguments of this method
        /// </summary>
        public HoxisProtocolArgs args;

        public HoxisProtocolAction(string methodArg, HoxisProtocolArgs argsArg)
        {
            method = methodArg;
            args = argsArg;
        }
    }
    public struct HoxisProtocolArgs
    {
        public static readonly HoxisProtocolArgs undef = new HoxisProtocolArgs { kv = new Dictionary<string, string>() };
        public Dictionary<string, string> kv;
        public HoxisProtocolArgs(Dictionary<string, string> kvArg) { kv = kvArg; }
        public HoxisProtocolArgs(params KVString[] kvs)
        {
            kv = new Dictionary<string, string>();
            foreach (KVString s in kvs) { kv.Add(s.key, s.val); }
        }
    }
}
