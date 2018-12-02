using System;
using System.Collections;
using System.Collections.Generic;

namespace DacLib.Hoxis {
    #region delegates
    public delegate void ProtocolHandler(HoxisProtocol proto);
    public delegate void ActionHandler(HoxisProtocolAction action);
    public delegate void ActionArgsHandler(HoxisProtocolArgs args);
    public delegate void ResponseHandler(HoxisProtocolArgs args, string handle);
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
        Player
    }
    public enum HoxisType
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
        Inteam,
        Matching,
        Playing
    }

    #endregion

    #region interfaces

    #endregion
}