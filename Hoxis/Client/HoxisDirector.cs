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

        public static int protocolQueueCapacity { get; set; }
        public static short protocolQueueProcessQuantity { get; set; }
        public event BytesForVoid_Handler onPost;

        protected Dictionary<string, ActionArgsHandler> respCbTable = new Dictionary<string, ActionArgsHandler>();
        private Dictionary<HoxisAgentID, HoxisAgent> _agentSearcher;
        private Queue<HoxisProtocol> _protocolQueue;

        void Awake()
        {
            if (Ins == null) Ins = this;
            _agentSearcher = new Dictionary<HoxisAgentID, HoxisAgent>();
            _protocolQueue = new Queue<HoxisProtocol>(protocolQueueCapacity);
        }

        void Update()
        {
            // Get protocols from queue and process
            ProcessInRound();
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
            lock (_protocolQueue) { _protocolQueue.Enqueue(proto); }
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

        private void ProcessInRound()
        {
            if (_protocolQueue.Count <= 0) return;
            short count = 0;
            while (count < protocolQueueProcessQuantity)
            {
                if (_protocolQueue.Count <= 0) break;
                HoxisProtocol proto = _protocolQueue.Dequeue();
                switch (proto.type) {
                    case ProtocolType.Response:
                        ReqHandle handle = FF.JsonToObject<ReqHandle>(proto.handle);
                        // todo 消除等待
                        // todo 处理错误
                        if (proto.err) { }
                        respCbTable[proto.action.method](proto.action.args);
                        break;
                    case ProtocolType.Synchronization:
                        HoxisAgent agent = GetAgent(proto.sender.aid);
                        if (agent != null) agent.CallBehaviour(proto.action);
                        break;
                    case ProtocolType.Proclamation:

                        break;
                }
                count++;
            }
        }

        private void OnPost(byte[] data) { if (onPost == null) return;onPost(data); }

        #endregion

        #region response callbacks
        private void SignInCb(HoxisProtocolArgs args)
        {
            string code = args["code"];
            if (code == Consts.RESP_RECONNECT)
            {
                // todo 提示重连
            }
            else {
                // todo 成功
            }
        }
        #endregion
    }
}