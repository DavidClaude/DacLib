using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DacLib.U3D.Archis
{
	public class GameObjPool
	{
		public int maxCount { get; }

		private GameObject[] _gameObjs;

		public GameObjPool (int maxCountArg = 1024)
		{
			maxCount = maxCountArg;
			_gameObjs = new GameObject[maxCount];
		}

		public GameObject Request (string name, string path)
		{
			GameObject go;
			if (name == "") {
				for (int i = 0; i < maxCount; i++) {
					if (_gameObjs [i]) {
						go = _gameObjs [i];
						_gameObjs [i] = null;
						go.GetComponent<GOElem> ().OnRequest ();
						return go;
					}
				}
			} else {
				for (int i = 0; i < maxCount; i++) {
					if (_gameObjs [i]) {
						if (_gameObjs [i].name == name) {
							go = _gameObjs [i];
							_gameObjs [i] = null;
							go.GetComponent<GOElem> ().OnRequest ();
							return go;
						}
					}
				}
			}
			go = Resources.Load (path) as GameObject;
			if (!go.GetComponent<GOElem> ()) {
				//log no GOElem
				return null;
			}
			go.GetComponent<GOElem> ().OnRequest ();
			return go;
		}

		public bool Collect (GameObject go)
		{
			if (!go.GetComponent<GOElem> ()) {
				//log no GOElem
				return false;
			}
			for (int i = 0; i < maxCount; i++) {
				if (_gameObjs [i] == null) {
					_gameObjs [i] = go;
					go.GetComponent<GOElem> ().OnCollect ();
					return true;
				}
			}
			return false;
		}
	}
	
}