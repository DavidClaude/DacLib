using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DacLib.Hoxis;
using DacLib.Hoxis.Client;
using DacLib.Generic;

public class TS_Actor : HoxisBehaviour
{

    public int ID;

    public string method;

    // Use this for initialization
    void Start()
    {
        
        behavTable.Add(method, PrintID);
        Debug.Log("I am a actor, the count is " + behavTable.Keys.Count);
        foreach (string s in behavTable.Keys) {
            Debug.Log("ID: " + ID + ", the methon name is " + s);
        }
        
    }

    public void PrintID(HoxisProtocol proto)
    {
        Debug.Log(ID);
    }
}
