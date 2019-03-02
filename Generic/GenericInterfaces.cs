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
    /// Include entering/Updating/Exit
    /// </summary>
    public interface IEUE
    {
        void OnEnter();
        void OnUpdate();
        void OnExit();
    }

    public interface IFSMNodal : IEUE
    {
        string name { get; }
        NodeType nodeType { get; }
        NodePort nodePort { get; set; }
    }

    /// <summary>
    /// Enable to be put into and out from most kinds of object pool 
    /// </summary>
    public interface IReusable
    {
        int localID { get; set; }
        bool isOccupied { get; set; }
        void OnRequest(object state);
        void OnRelease();
    }

    public interface IStatusControllable
    {
        void Awake();
        void Pause();
        void Continue();
        void Reset();
    }

    public interface IInitializable
    {
        void Initialize();
    }
}
