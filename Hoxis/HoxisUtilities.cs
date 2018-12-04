using System;
using System.Collections;
using System.Collections.Generic;

namespace DacLib.Hoxis {
    #region delegates
    public delegate void ProtocolHandler(HoxisProtocol proto);
    public delegate void ActionHandler(HoxisProtocolAction action);
    public delegate void ActionArgsHandler(HoxisProtocolArgs args);
    public delegate bool ResponseHandler(string handle, HoxisProtocolArgs args);
    #endregion

    #region enums
    public enum ProtocolType
    {
        None = 0,
        Synchronization,
        Request,
        Response
    }
    public enum ReceiverType
    {
        None = 0,
        Server,
        Cluster,
        Team,
        User
    }
    public enum AgentType
    {
        None = 0,
        Host,
        Proxied,
        Perpetual
    }
    /// <summary>
    /// State that server keeps to decide how to communicate
    /// </summary>
    public enum UserState
    {
        None = 0,
        Main,
        Reconnecting
    }

    #endregion

    #region interfaces

    #endregion

    #region constants
    public class Consts {
        public const string RESP_SUCCESS = "0";
        public const string RESP_RECONNECT = "10001";
    }
    #endregion
}