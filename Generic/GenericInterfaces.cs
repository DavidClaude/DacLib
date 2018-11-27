using System;

namespace DacLib.Generic
{

    /// <summary>
    /// Enable to extract and load json data of its information and states
    /// </summary>
    public interface IJsonable
    {
        string ToJson();
        void LoadJson(string json);
    }

    /// <summary>
    /// Enable to be served by specific object, such as CriticalReception
    /// </summary>
    public interface IReceivable
    {
        bool isUpdated { get; set; }
        void OnAccept();
        void OnService();
        void OnDecline();
    }

    /// <summary>
    /// Enable to be put into and out from most kinds of object pool 
    /// </summary>
    public interface IReusable
    {
        bool isOccupied { get; set; }
        void OnRequest(object state);
        void OnRelease();
    }
}
