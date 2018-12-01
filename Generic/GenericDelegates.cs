using System;

namespace DacLib.Generic
{
    #region generic delegates
    public delegate void NoneForVoid_Handler();
    public delegate void ObjectForVoid_Handler(object state);
    public delegate void IntForVoid_Handler(int i);
    public delegate void StringForVoid_Handler(string s);
    public delegate void BytesForVoid_Handler(byte[] data);
    public delegate void RetForVoid_Handler(Ret ret);
    public delegate void NoneForOutRet_Handler(out Ret ret);
    #endregion
}