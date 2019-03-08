using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DacLib.Generic;
using DacLib.U3D.Generic;
using System;

namespace DacLib.U3D.Archits.GOPool
{
    public class GOPool_Manager
    {

        /// <summary>
        /// Crude prefab
        /// </summary>
        public GameObject prefab { get; }

        /// <summary>
        /// Events of requesting and collecting one GameObject
        /// </summary>
        public event GameObjectForVoid onRequest, onCollect;

        private List<GameObject> _elems;

        public GOPool_Manager(string path, out Ret ret)
        {
            prefab = Resources.Load(path) as GameObject;
            if (prefab == null) { ret = new Ret(LogLevel.Error, 1, "prefab path is invalid"); return; }
            _elems = new List<GameObject>();
            ret = Ret.ok;
        }

        public GameObject Request(string name)
        {
            GameObject go_ret = null;

            // Try to get an available one with the same name
            foreach (GameObject go in _elems) { if (go.name == name) { go_ret = go; break; } }
            if (go_ret != null)
            {
                _elems.Remove(go_ret);
                go_ret.SetActive(true);
                OnRequest(go_ret);
                return go_ret;
            }

            // If failed, instantiate the prefab
            go_ret = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity);
            OnRequest(go_ret);
            // Set the name
            go_ret.name = name;
            return go_ret;
        }

        public void Collect(GameObject go)
        {
            OnCollect(go);
            go.SetActive(false);
            if (_elems.Contains(go)) return;
            _elems.Add(go);
        }

        private void OnRequest(GameObject go) { if (onRequest == null) return; onRequest(go); }
        private void OnCollect(GameObject go) { if (onCollect == null) return; onCollect(go); }
    }
}
