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

    //private static Dictionary<string, ProtocolHandler> actionTable;

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
        //HoxisProtocol proto = new HoxisProtocol
        //{
        //    type = ProtocolType.Synchronization,
        //    rcvr = new HoxisProtocolReceiver
        //    {
        //        type = ReceiverType.Cluster,
        //        id = new HoxisID("survivor", 24),
        //    },
        //    sndr = new HoxisProtocolSender
        //    {
        //        id = new HoxisID("soldier", 1),
        //        back = true,
        //    },
        //    action = new HoxisProtocolAction
        //    {
        //        mthd = "move",
        //        args = new Dictionary<string, string>() {
        //            { "speed","5.564346464314793131544987"},
        //        }
        //    },
        //    desc = "test",
        //};
        //string json = FormatFunc.ObjectToJson(proto);
        //Debug.Log("Protocol: " + json);
        //HoxisProtocol rcvProto = FormatFunc.JsonToObject<HoxisProtocol>(json);
        //Debug.Log("Receive protocol: " + FormatFunc.ObjectToJson(rcvProto));

        //Dictionary<string, ProtocolHandler> actions = new Dictionary<string, ProtocolHandler>();
        //actions.Add("move", Move);
        //actions.Add("attack", Attack);

        //string mthd = rcvProto.action.mthd;
        //Debug.Log("Method: " + mthd);
        //Debug.Log("Type: " + rcvProto.type);

        //actions[mthd](rcvProto.action.args);

        //字典中struct等价测试
        //HoxisID id0 = new HoxisID { group = "g1", id = 0 };
        //HoxisID id1 = new HoxisID { group = "g1", id = 1 };
        //HoxisID id2 = new HoxisID { group = "g1", id = 2 };
        //HoxisID id3 = new HoxisID { group = "g2", id = 0 };
        //HoxisID id4 = new HoxisID { group = "g2", id = 1 };

        //Dictionary<HoxisID, string> hidTable = new Dictionary<HoxisID, string>() {
        //    { id0, "id0"},
        //    { id1, "id1"},
        //    { id2, "id2"},
        //    { id3, "id3"},
        //    { id4, "id4"}
        //};
        //HoxisID id = new HoxisID { group = "g3", id = 2 };
        //Debug.Log((hidTable.ContainsKey(id) ? true : false));

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Move(Dictionary<string,string> args) {
        double speed = double.Parse(args["speed"]);
        Debug.Log("Move: " + speed);

    }

    public void Attack(Dictionary<string, string> args) {
        int idInt = int.Parse(args["id"]);
        Debug.Log("Attack: " + idInt);
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
