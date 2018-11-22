using System;

namespace DacLib.Generic
{
	public delegate void NoneForVoid_Handler ();
	public delegate void IntForVoid_Handler (int i);
    public delegate void StringForVoid_Handler(string s);
	public delegate void GameObjectForVoid_Handler (UnityEngine.GameObject gameObj);
}