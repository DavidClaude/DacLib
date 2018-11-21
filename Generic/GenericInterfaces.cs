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
        void OnServiceStart();
        void OnService();
        void OnServiceStop();
    }
}
