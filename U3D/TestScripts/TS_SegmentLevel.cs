using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DacLib.Generic;
using DacLib.Codex;

public class TS_SegmentLevel : MonoBehaviour
{
    SegmentLevel sl = new SegmentLevel(5);
    public float val;

    // Use this for initialization
    void Start()
    {
        Ret ret;
        sl.SetThreshold(0, 10, out ret);
        if (ret.code != 0)
            Debug.LogError(ret.desc);
        sl.SetThreshold(1, 20, out ret);
        if (ret.code != 0)
            Debug.LogError(ret.desc);
        sl.SetThreshold(2, 25, out ret);
        if (ret.code != 0)
            Debug.LogError(ret.desc);
        sl.SetThreshold(3, 35, out ret);
        if (ret.code != 0)
            Debug.LogError(ret.desc);
        sl.SetThreshold(4, 50, out ret);
        if (ret.code != 0)
            Debug.LogError(ret.desc);

        Debug.Log(sl.level);
        Debug.Log(sl.degree);

        sl.onLevelChange += (i) =>
        {
            Debug.Log("Level change to: " + i);
        };
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            sl.Extend(val);
            Debug.Log("Degree: " + sl.degree);
            Debug.Log("Local rate: " + sl.localProgress);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            sl.Recover();
            Debug.Log("Degree: " + sl.degree);
            Debug.Log("Local rate: " + sl.localProgress);
        }
    }
}
