using System;
using System.Collections.Generic;

namespace DacLib.Hoxis
{
    public struct HoxisProtocol
    {
        /// <summary>
        /// Default and invalid HoxisProtocol
        /// </summary>
        public static readonly HoxisProtocol nil = new HoxisProtocol
        {
            type = ProtocolType.None,
            rcvr = HoxisProtocolReceiver.nil,
            sndr = HoxisProtocolSender.nil,
            action = HoxisProtocolAction.nil,
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
        /// Whom will receive this protocol
        /// </summary>
        public HoxisProtocolReceiver rcvr;

        /// <summary>
        /// Who sends this protocol
        /// </summary>
        public HoxisProtocolSender sndr;

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
        public static readonly HoxisProtocolReceiver nil = new HoxisProtocolReceiver { type = ReceiverType.None, hid = HoxisID.nil };

        /// <summary>
        /// Server, cluster of current game, teammates, or some player ?
        /// </summary>
        public ReceiverType type;
        
        /// <summary>
        /// If to some player, what's his HoxisID
        /// </summary>
        public HoxisID hid;
    }
    public struct HoxisProtocolSender
    {
        public static readonly HoxisProtocolSender nil = new HoxisProtocolSender { hid = HoxisID.nil, loopback = true };

        /// <summary>
        /// HoxisID of sender
        /// </summary>
        public HoxisID hid;

        /// <summary>
        /// Should server return this protocol when broadcasting ?
        /// </summary>
        public bool loopback;
    }
    public struct HoxisProtocolAction
    {
        public static readonly HoxisProtocolAction nil = new HoxisProtocolAction { method = "", args = null };

        /// <summary>
        /// Method name in actionTable
        /// </summary>
        public string method;

        /// <summary>
        /// Arguments of this method
        /// </summary>
        public Dictionary<string, string> args;
    }
}
