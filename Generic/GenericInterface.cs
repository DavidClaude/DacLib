using System;

namespace DacLib.Generic {

    /// <summary>
    /// 可发送/接收Json数据接口
    /// </summary>
    public interface IJsonable {
        string GenerateJson();
        void LoadJson(string json);
    }
}
