using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DacLib.Hoxis;
using DacLib.Hoxis.Client;

public class TS_Survivor : HoxisBehaviour
{
    public int ID;

    private void Start()
    {
        behavTable.Add("S0", PrintID);
        behavTable.Add("S1", PrintID);
        behavTable.Add("S2", PrintID);
        behavTable.Add("S3", PrintID);
        behavTable.Add("S4", PrintID);
        behavTable.Add("S5", PrintID);
        Debug.Log("I am a survivor, the count is " + behavTable.Keys.Count);
        foreach (string s in behavTable.Keys)
        {
            Debug.Log("ID: " + ID + ", the methon name is " + s);
        }
    }

    public void PrintID(HoxisProtocol proto)
    {
        Debug.Log(ID);
    }
}
