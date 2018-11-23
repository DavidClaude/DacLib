using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DacLib.Generic;
using DacLib.Hoxis;
using DacLib.Hoxis.Client;

public static class HoxisUnityLauncher
{
    #region ret codes
    public const byte RET_INVALID_PREFAB_PATH = 1;
    public const byte RET_PREFAB_WITHOUT_HOXIS_BEHAVIOUR = 2;
    #endregion

    /// <summary>
    /// The sample process of awaking Hoxis in playing scenes
    /// </summary>
    public static void Awake(out Ret ret)
    {
        // Register methods to events of HoxisClient
        HoxisClient.onInitError += (ret0) => { Debug.LogError(ret0.desc); };
        HoxisClient.onConnected += () => { Debug.Log("Connect success"); };
        HoxisClient.onConnectError += (ret0) => { Debug.LogError(ret0.desc); };
        HoxisClient.onReceiveError += (ret0) => { Debug.LogError(ret0.desc); };
        HoxisClient.onSendError += (ret0) => { Debug.LogError(ret0.desc); };

        // Init HoxisClient
        HoxisClient.InitConfig(out ret);
        if (ret.code != 0) return;
        // Connect
        HoxisClient.Connect();
    }

    /// <summary>
    /// Create a GameObject with HoxisAgent
    /// </summary>
    /// <param name="prefabPath"></param>
    /// <param name="parent"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <param name="htp"></param>
    /// <param name="hid"></param>
    /// <param name="autoSyn"></param>
    /// <param name="ret"></param>
    /// <returns></returns>
    public static GameObject CreateHoxisGameObject(string prefabPath, Transform parent, Vector3 position, Quaternion rotation, HoxisType htp, HoxisID hid, bool autoSyn, out Ret ret)
    {
        // Load the prefab resource
        GameObject prefab = Resources.Load(prefabPath) as GameObject;
        if (prefab == null)
        {
            ret = new Ret(LogLevel.Error, RET_INVALID_PREFAB_PATH, "Prefab path:" + prefabPath + " is invalid");
            return null;
        }
        // Instantiate
        GameObject hgo = GameObject.Instantiate(prefab, position, rotation);
        // If parent is null, it means the position and rotation are in world space
        // Otherwise, in local space
        if (parent != null)
        {
            hgo.transform.SetParent(parent);
            hgo.transform.localPosition = position;
            hgo.transform.localRotation = rotation;
        }
        // Check the HoxisBehaviour
        if (hgo.GetComponent<HoxisBehaviour>() == null)
        {
            ret = new Ret(LogLevel.Error, RET_PREFAB_WITHOUT_HOXIS_BEHAVIOUR, "Prefab:" + hgo.name + "doesn't have HoxisBehavour");
            return null;
        }
        // Check the HoxisAgent, add one if null
        HoxisAgent agent = hgo.GetComponent<HoxisAgent>();
        if (agent == null) { agent = hgo.AddComponent<HoxisAgent>(); }
        // Call the construct function
        agent.CoFunc(htp, hid, autoSyn);
        // Register to HoxisDirector 
        HoxisDirector.Register(agent, out ret);
        if (ret.code != 0) { return null; }
        ret = Ret.ok;
        return hgo;
    }
}
