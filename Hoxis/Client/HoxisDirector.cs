using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DacLib.Generic;
using DacLib.Codex;

using FF = DacLib.Generic.FormatFunc;

namespace DacLib.Hoxis.Client
{
    /// <summary>
    /// Manager of all HoxisAgents in current cluster
    /// Major in:
    /// -manage agents
    /// -search for a given agent or its gameObject
    /// -parse protocols from HoxisClient, then send them to correct agents
    /// -wrapp and post a protocol
    /// -etc.
    /// </summary>
    public class HoxisDirector :MonoBehaviour
    {
        #region ret codes
        public const byte RET_HID_EXISTS = 1;
        public const byte RET_NO_UNOCCUPIED_HID = 2;
        public const byte RET_NO_HID = 3;
        #endregion

        public static HoxisDirector Ins { get; private set; }

        public static int actionQueueCapacity { get; set; }
        public static short actionQueueProcessQuantity { get; set; }

        public event BytesForVoid_Handler onPost;

        private Dictionary<HoxisAgentID, HoxisAgent> _agentSearcher;
        private FiniteProcessQueue<HoxisProtocol> _actionQueue;

        void Awake()
        {
            if (Ins == null) Ins = this;
            _agentSearcher = new Dictionary<HoxisAgentID, HoxisAgent>();
            _actionQueue = new FiniteProcessQueue<HoxisProtocol>(actionQueueCapacity, actionQueueProcessQuantity);
            _actionQueue.onProcess += (object state) => {
                HoxisProtocol proto = (HoxisProtocol)state;
                Debug.Log(FF.ObjectToJson(proto));
            };
        }

        void Update()
        {
            if (_actionQueue != null) { _actionQueue.ProcessInRound(); }
        }

        /// <summary>
        /// Add an agent to searcher
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="ret"></param>
        public void Register(HoxisAgent agent, out Ret ret)
        {
            HoxisAgentID hid = agent.id;
            if (_agentSearcher.ContainsKey(hid))
            {
                ret = new Ret(LogLevel.Warning, RET_HID_EXISTS, "HoxisID:" + hid.group + "," + hid.id + " already exists");
                return;
            }
            _agentSearcher.Add(hid, agent);
            ret = Ret.ok;
        }

        /// <summary>
        /// Remove an agent to searcher
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ret"></param>
        public void Remove(HoxisAgentID id, out Ret ret)
        {
            if (!_agentSearcher.ContainsKey(id))
            {
                ret = new Ret(LogLevel.Warning, RET_NO_HID, "HoxisID:" + id.group + "," + id.id + " doesn't exist");
                return;
            }
            _agentSearcher.Remove(id);
            ret = Ret.ok;
        }

        public void Remove(HoxisAgent agent, out Ret ret)
        {
            HoxisAgentID hid = agent.id;
            Remove(hid, out ret);
        }

        /// <summary>
        /// Search for an agent through searcher
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public HoxisAgent GetAgent(HoxisAgentID id, out Ret ret)
        {
            if (!_agentSearcher.ContainsKey(id))
            {
                ret = new Ret(LogLevel.Warning, RET_NO_HID, "HoxisID:" + id.group + "," + id.id + " doesn't exist");
                return null;
            }
            ret = Ret.ok;
            return _agentSearcher[id];
        }

        public HoxisAgent GetAgent(HoxisAgentID id)
        {
            Ret ret;
            return GetAgent(id, out ret);
        }

        /// <summary>
        /// **WITHIN THREAD**
        /// The entrance of protocol bytes
        /// Called by HoxisClient
        /// </summary>
        /// <param name="data"></param>
        public void ProtocolEntry(byte[] data)
        {
            string json = FF.BytesToString(data);
            HoxisProtocol proto = FF.JsonToObject<HoxisProtocol>(json);
            lock (_actionQueue) { _actionQueue.Enqueue(proto); }
        }

        /// <summary>
        /// The poster of protocol
        /// Convert protocol to protocol bytes
        /// </summary>
        /// <param name="proto"></param>
        public void ProtocolPost(HoxisProtocol proto)
        {
            switch (proto.type)
            {
                case ProtocolType.Synchronization:
                    
                    break;
                case ProtocolType.Request:
                    // todo wait
                    break;
                case ProtocolType.Response:
                    
                    break;
            }
            string json = FF.ObjectToJson(proto);
            byte[] data = FF.StringToBytes(json);
            OnPost(data);
        }

        #region private functions

        private void OnPost(byte[] data) { if (onPost == null) return;onPost(data); }

        #endregion
    }
}