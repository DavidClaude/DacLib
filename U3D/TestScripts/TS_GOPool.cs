using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DacLib.Generic;
using DacLib.U3D.Archits.GOPool;


public class TS_GOPool : MonoBehaviour {

    public string name = "audio0";

    GOPool_Manager pool;
    private GameObject gameObj;

	// Use this for initialization
	void Start () {
        Ret ret;
        pool = new GOPool_Manager("Prefabs/Audio or Bullet", out ret);
        if (ret.code != 0) { Debug.Log(ret.desc); }
        pool.onRequest += (go) => { Debug.Log("Request: " + go.name); go.AddComponent<AudioSource>(); };
        pool.onCollect += (go) => { Debug.Log("Collect: " + go.name); };
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.R))
        {
            gameObj = pool.Request(name);
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            pool.Collect(gameObj);
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            gameObj = new GameObject("new one");
        }
	}
}
