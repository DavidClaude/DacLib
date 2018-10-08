using System;
using System.Collections;
using System.Collections.Generic;

namespace DacLib.Hoxis
{
	/// <summary>
	/// 负责游戏场景中的同步
	/// </summary>
	public static class Hoxis_SynDirector
	{
//		public static void Init (Hoxis_Agent host) {
//			_hidToAgents = new Dictionary<string, Dictionary<int, Hoxis_Agent>> ();
//		}


		private static Dictionary<string, Dictionary<int, Hoxis_Agent>> _hidToAgents;

		private static Hoxis_Agent GetAgent (Hoxis_ID hid) {
			if (!_hidToAgents.ContainsKey (hid.group))
				return null;
			Dictionary<int, Hoxis_Agent> dict = _hidToAgents [hid.group];
			if (dict == null)
				return null;
			if (!dict.ContainsKey (hid.index))
				return null;
			Hoxis_Agent agent = dict [hid.index];
			return agent;
		}



		//具体调用方法的功能需使用链接的方式，解耦
	}

	public class Hoxis_ID
	{
		public string group { get;}
		public int index { get;}
		public Hoxis_ID (string groupArg, int idArg)
		{
			group = groupArg;
			index = idArg;
		}
	}



}