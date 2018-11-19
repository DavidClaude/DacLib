﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DacLib.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using DacLib.Hoxis;

public class TS_Func : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        //转Table测试
        //MyInfo mi = new MyInfo("David", 18, 100f);
        //string json = FormatFunc.ObjectToJson(mi);

        //Dictionary<string, object> dataTable = FormatFunc.JsonToTable(json);


        //Dictionary<string, float> constTrace = new Dictionary<string, float>();
        //constTrace.Add("weapon0", 10f);
        //constTrace.Add("weapon1", 20f);
        //constTrace.Add("weapon2", 30f);
        //dataTable.Add("constTrace", constTrace);
        //Debug.Log(FormatFunc.ObjectToJson(dataTable));

        //IJsonble接口测试
        //Property p = new Property(50f, -50f, 200f);
        //p.AddTrace("equipment0", 15f);
        //p.AddTrace("equipment1", -3f);
        //p.AddTrace("equipment2", 22.5f);
        //p.AddTrace("equipment3", 0.15f, CalcMode.Percentage);
        //p.AddTrace("equipment4", 0.35f, CalcMode.Percentage);
        //Debug.Log("Val is " + p.val);
        //string json = (p as IJsonable).ToJson();
        //Debug.Log("ToJson:" + json);
        //Property pn = new Property(50f, -50f, 200f);
        //(pn as IJsonable).LoadJson(json);
        //Debug.Log("New json ToJson:" + (pn as IJsonable).ToJson());
        //Debug.Log("New val is " + pn.val);
        //Indicator ind = new Indicator(50f);
        //ind.Change(15f);
        //Debug.Log("Val is " + ind.val);
        //string json = (ind as IJsonable).ToJson();
        //Debug.Log("ToJson:" + json);
        //Indicator indn = new Indicator(50f);
        //(indn as IJsonable).LoadJson(json);
        //Debug.Log("New json ToJson:" + (indn as IJsonable).ToJson());
        //Debug.Log("New val is " + indn.val);

        //HoxisProtocol测试    
        HoxisProtocol proto = new HoxisProtocol
        {
            type = ProtocolType.Synchronization,
            rcvr = new HoxisProtocolReceiver {
                type = ReceiverType.MultiPlayers,
                id = new HoxisID("survivor",24),              
            },
            sndr = new HoxisProtocolSender {
                id = new HoxisID("survivor",23),
                back = true,
            },
            action = new HoxisProtocolAction {
                mthd = "move",
                args = new Dictionary<string, object>() {
                    { "speed",5f},
                }
            },
            desc = "test",
        };
        string json = FormatFunc.ObjectToJson(proto);
        Debug.Log("Protocol: " + json);
        HoxisProtocol rcvProto = FormatFunc.JsonToObject<HoxisProtocol>(json);
        Debug.Log("Receive protocol: " + FormatFunc.ObjectToJson(rcvProto));

        Dictionary<string, ProtocolHandler> actions = new Dictionary<string, ProtocolHandler>();
        actions.Add("move", Move);
        actions.Add("attack", Attack);

        string mthd = rcvProto.action.mthd;
        Debug.Log("Method: " + mthd);

        actions[mthd](rcvProto);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Move(HoxisProtocol proto) {

        
    }

    public void Attack(HoxisProtocol proto) {
        Debug.Log((int)proto.action.args["id"]);
    }
}

public class MyInfo {
    public string name;
    public int age;
    public float ability;
    public MyInfo(string nameArg, int ageArg, float abilityArg)
    {
        name = nameArg;
        age = ageArg;
        ability = abilityArg;
    }
}
