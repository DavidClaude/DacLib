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
    public enum UserState
    {
        /// <summary>
        /// Unused
        /// </summary>
        None = 0,
        /// <summary>
        /// In the main scene
        /// </summary>
        Main,
        /// <summary>
        /// In a team
        /// </summary>
        Inteam,
        /// <summary>
        /// Matching for playing
        /// </summary>
        Matching,
        /// <summary>
        /// Playing
        /// </summary>
        Playing,
        /// <summary>
        /// Waiting for reconnecting after disconnected with exception
        /// </summary>
        Waiting
    }

    #endregion

    #region interfaces

    #endregion
}