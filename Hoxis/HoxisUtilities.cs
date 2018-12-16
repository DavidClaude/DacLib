using System;
using System.Collections;
using System.Collections.Generic;

namespace DacLib.Hoxis {
    #region delegates
    public delegate void ProtocolHandler(HoxisProtocol proto);
    public delegate void ActionHandler(HoxisProtocolAction action);
    public delegate void ActionArgsHandler(HoxisProtocolArgs args);
    public delegate bool ResponseHandler(string handle, HoxisProtocolArgs args);
    public delegate void ErrorHandler(string err, string desc);
    #endregion

    #region enums
    public enum ProtocolType
    {
        None = 0,
        Synchronization,    // One client sends -> Server transmits -> All clients receive
        Request,            // One client sends -> Server responses -> This client receive
        Response,           // ...
        Proclamation        // Server(only) sends -> All clients receive
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
    public enum UserConnectionState
    {
        None = 0,       // Offline or connected but no user information
        Default,        // Signed in but not necessary to record realtime data, it means that server won't recover its realtime data if reconnecting
        Active,         // Necessary to record realtime data
        Disconnected    // Network anormaly, waiting for reconnecting
    }
    public enum ManageOperation
    {
        None = 0,
        Create,
        Join,
        Leave,
        Destroy
    }

    #endregion

    #region interfaces

    #endregion

    #region constants
    public class Consts {
        public const string RESP_SUCCESS = "0";
        public const string RESP_NO_USER_INFO = "10001";
        public const string RESP_RECONNECT = "10002";
        public const string RESP_HEARTBEAT_UNAVAILABLE = "10003";
        public const string RESP_CHECK_FAILED = "10004";
    }
    #endregion
}