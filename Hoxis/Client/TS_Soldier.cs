using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DacLib.Generic;
using DacLib.Hoxis;
using DacLib.Hoxis.Client;

public class TS_Soldier : HoxisBehaviour {
    
    void Awake()
    {
        behavTable.Add("shoot", Shoot);
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void Shoot(Dictionary<string, string> args)
    {
        Debug.Log("Hi, I'm attacked, args are " + args["val"] + ", " + args["src"]);
        this.name = "hahaha";
    }
}
