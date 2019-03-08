using System;

namespace DacLib.Generic
{
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
    public interface ICritical
    {
        int localID { get; set; }
        bool isOccupied { get; set; }
        void OnRequest(object state);
        void OnRelease();
    }

    public interface IReusable
    {
        void OnRequest();
        void OnCollect();
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
