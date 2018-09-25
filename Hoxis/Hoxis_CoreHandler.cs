using System;
using System.Collections;
using System.Collections.Generic;

namespace DacLib.Hoxis
{
	public static class Hoxis_CoreHandler
	{
		private static Dictionary<string, Dictionary<int, Hoxis_Agent>> _hidToAgents = new Dictionary<string, Dictionary<int, Hoxis_Agent>> ();

		private static Hoxis_Agent GetAgent (Hoxis_ID hid) {
			if (!_hidToAgents.ContainsKey (hid.group))
				return null;
			Dictionary<int, Hoxis_Agent> dict = _hidToAgents [hid.group];
			if (dict == null)
				return null;
			if (!dict.ContainsKey (hid.id))
				return null;
			Hoxis_Agent agent = dict [hid.id];
			return agent;
		}







		//具体调用方法的功能需使用链接的方式，解耦
	}

	public class Hoxis_ID
	{
		public string group { get;}
		public int id { get;}
		public Hoxis_ID (string groupArg, int idArg)
		{
			group = groupArg;
			id = idArg;
		}
	}



}