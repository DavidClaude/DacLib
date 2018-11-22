using System;
using System.Collections;
using System.Collections.Generic;

namespace DacLib.Hoxis {
    #region delegates
    public delegate void ActionHandler(Dictionary<string, string> args);

    #endregion

    #region enums
    public enum ProtocolType
    {
        None = 0,
        Synchronization = 1,
        Request = 2,
        Response = 3
    }
    public enum ReceiverType
    {
        None = 0,
        Server = 1,
        Cluster = 2,
        Team = 3,
        Player = 4
    }
    public enum HoxisType
    {
        None = 0,
        Host = 1,
        Proxied = 2,
        Perpetual = 3
    }

    #endregion

    #region configurations(constants)
    public static class Configs
    {
        public static string hoxisPath { get { return UnityEngine.Application.dataPath + "/DacLib/Hoxis/"; } }
    }
    #endregion

    #region interfaces

    #endregion
}
