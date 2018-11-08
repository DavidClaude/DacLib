using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DacLib.Codex;
using DacLib.Generic;

public class TS_Toml : MonoBehaviour
{



	// Use this for initialization
	void Start ()
	{
		Ret ret;
		TomlConfiguration tc = new TomlConfiguration (Application.dataPath + "/DacLib/Codex/TSP_Toml.toml", out ret);
		if (ret.code == 0) {
			foreach (string sec in tc.GetSections()) {
				Debug.Log (sec);
			}
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
}
