using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DacLib.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

public class TS_Func : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        MyInfo mi = new MyInfo("David", 18, 100f);
        string json = FormatFunc.ObjectToJson(mi);

        Dictionary<string, object> dataTable = FormatFunc.JsonToTable(json);


        Dictionary<string, float> constTrace = new Dictionary<string, float>();
        constTrace.Add("weapon0", 10f);
        constTrace.Add("weapon1", 20f);
        constTrace.Add("weapon2", 30f);
        dataTable.Add("constTrace", constTrace);
        Debug.Log(FormatFunc.ObjectToJson(dataTable));

    }

    // Update is called once per frame
    void Update()
    {

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
