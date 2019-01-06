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
    public delegate void ExceptionHandler(int code, string message);
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
        Disconnected    // Network anomaly, waiting for reconnecting
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
        // Hoxis information
        public const string VERSION = "0.0.0";

        // response codes
        public const string RESP_SUCCESS = "0";                     // response success
        public const string RESP_ILLEGAL_ARGUMENT = "10001";        // argument (in HoxisProtocolArgs) is illegal
        public const string RESP_NO_USER_INFO = "10002";            // no user information on server
        public const string RESP_HEARTBEAT_UNAVAILABLE = "10003";   // heartbeat has been stopped
        public const string RESP_CHECK_FAILED = "10004";            // request is illegal
        public const string RESP_ACTIVATED_ALREADY = "10005";       // connection state is active already
        public const string RESP_SET_DEFAULT_ALREADY = "10006";     // connection state is default already
        public const string RESP_SET_STATE_UNABLE = "10007";        // unable to activate state, mostly because of incorrect state

        // exception codes
        public const int CODE_HEARTBEAT_TIMEOUT = 20001;            // heartbeat monitor throws timeout

        // server affairs
        public const int AFFAIR_RELEASE_CONNECTION = 30001;         // affair for releasing a connection

        // client affairs
        public const int AFFAIR_INIT_ERROR = 31001;                 // affair for initializing error
        public const int AFFAIR_CONNECT = 31002;                    // affair for connecting success
        public const int AFFAIR_CONNECT_ERROR = 31003;              // affair for connecting error
        public const int AFFAIR_CLOSE = 31004;                      // affair for closing success
        public const int AFFAIR_CLOSE_ERROR = 31005;                // affair for closing error
        public const int AFFAIR_NETWORK_ANOMALY = 31006;                // affair for network anomaly
    }
    #endregion
}