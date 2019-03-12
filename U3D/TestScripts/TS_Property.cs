using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DacLib.Generic;


public class TS_Property : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Property p = new Property(50f);
        p.AddTrace("weapon1",10f);
        p.AddTrace("weapon1", 0.3f, DeltaMode.Percentage);
        Debug.Log(FormatFunc.ObjectToJson(p));

        Property p1 = FormatFunc.JsonToObject<Property>(FormatFunc.ObjectToJson(p));
        Debug.Log(FormatFunc.ObjectToJson(p1));
        Debug.Log(p1.value);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
