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
    public enum UserState
    {
        None = 0,       // Offline
        Main,           // Default status
        Served,         // With realtime status
        Reconnecting    // Disconnected (waiting for reconnection)
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
        public const string RESP_RECONNECT = "10001";
    }
    #endregion
}