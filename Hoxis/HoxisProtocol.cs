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
        public static readonly HoxisProtocolReceiver undef = new HoxisProtocolReceiver { type = ReceiverType.None, uid = 0 };
        public static readonly HoxisProtocolReceiver cluster = new HoxisProtocolReceiver
        {
            type = ReceiverType.Cluster,
            uid = 0
        };
        public static readonly HoxisProtocolReceiver team = new HoxisProtocolReceiver
        {
            type = ReceiverType.Team,
            uid = 0
        };

        /// <summary>
        /// Server, cluster of current game, teammates, or some player ?
        /// </summary>
        public ReceiverType type;
        
        /// <summary>
        /// If to some player, what's his user id
        /// </summary>
        public long uid;

        public HoxisProtocolReceiver(ReceiverType typeArg, long uidArg)
        {
            type = typeArg;
            uid = uidArg;
        }
    }
    public struct HoxisProtocolSender
    {
        public static readonly HoxisProtocolSender undef = new HoxisProtocolSender { uid = 0, aid = HoxisAgentID.undef, loopback = true };

        /// <summary>
        /// user id of sender
        /// </summary>
        public long uid;

        /// <summary>
        /// HoxisAgentID of sender
        /// </summary>
        public HoxisAgentID aid;

        /// <summary>
        /// Should server return this protocol when broadcasting ?
        /// </summary>
        public bool loopback;

        public HoxisProtocolSender(long uidArg, HoxisAgentID aidArg, bool loopbackArg = true)
        {
            uid = uidArg;
            aid = aidArg;
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

        public HoxisProtocolAction(string methodArg)
        {
            method = methodArg;
            args = HoxisProtocolArgs.undef;
        }
    }
    public struct HoxisProtocolArgs
    {
        public string this[string key] {
            get {
                if (!values.ContainsKey(key)) return "";
                return (values[key]);
            }
        }
        public static readonly HoxisProtocolArgs undef = new HoxisProtocolArgs { values = new Dictionary<string, string>() };
        public Dictionary<string, string> values;
        public HoxisProtocolArgs(Dictionary<string, string> kvArg) { values = kvArg; }
        public HoxisProtocolArgs(params KVString[] kvs)
        {
            values = new Dictionary<string, string>();
            foreach (KVString s in kvs) { values.Add(s.key, s.val); }
        }
    }
}
