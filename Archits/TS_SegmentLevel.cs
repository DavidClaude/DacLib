using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DacLib.Archits;

public class TS_SegmentLevel : MonoBehaviour {
	SegmentLevel sl = new SegmentLevel (5);
	public float val;

	// Use this for initialization
	void Start () {
		sl.SetThreshold (0, 1f);
		sl.SetThreshold (1, 2f);
		sl.SetThreshold (2, 3f);
		sl.SetThreshold (3, 4f);
		sl.SetThreshold (4, 5f);
		sl.onLevelChange += (i) => {
			Debug.Log ("Level change to: " + i);
		};
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.A)) {
			sl.Extend (val);
			Debug.Log ("Degree: " + sl.degree);
		}
		if (Input.GetKeyDown (KeyCode.I)) {
			sl.Init ();
			Debug.Log ("Degree: " + sl.degree);
		}
	}
}
