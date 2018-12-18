using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DacLib.Generic;
using DacLib.Codex;

using FF = DacLib.Generic.FormatFunc;
using SF = DacLib.Generic.SystemFunc;
using C = DacLib.Hoxis.Consts;

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
    public class HoxisDirector : MonoBehaviour
    {
        #region ret codes
        public const byte RET_HID_EXISTS = 1;
        public const byte RET_NO_UNOCCUPIED_HID = 2;
        public const byte RET_NO_HID = 3;
        #endregion

        public static HoxisDirector Ins { get; private set; }

        public static int protocolQueueCapacity { get; set; }
        public static short protocolQueueProcessQuantity { get; set; }
        public static float heartbeatInterval { get; set; }
        public bool isActive { get; private set; }
        public event ErrorHandler onResponseError;

        protected Dictionary<string, ActionArgsHandler> respCbTable;
        private Dictionary<HoxisAgentID, HoxisAgent> _agentSearcher;
        private FiniteProcessQueue<HoxisProtocol> _protoQueue;
        private RegularTimer _heartbeatTimer;

        /// <summary>
        /// Initially evaluation
        /// Auto
        /// </summary>
        void Awake()
        {
            if (Ins == null) Ins = this;
            isActive = false;
            respCbTable = new Dictionary<string, ActionArgsHandler>();
            _agentSearcher = new Dictionary<HoxisAgentID, HoxisAgent>();
        }

        /// <summary>
        /// Register
        /// Auto
        /// </summary>
        void Start()
        {
            respCbTable.Add("QueryConnectionStateCb", QueryConnectionStateCb);
            respCbTable.Add("SignInCb", SignInCb);
            respCbTable.Add("SignOutCb", SignOutCb);
            respCbTable.Add("ReconnectCb", ReconnectCb);
            respCbTable.Add("RefreshHeartbeatCb", RefreshHeartbeatCb);
            HoxisClient.onConnected += () => { _heartbeatTimer.Start(); };
            HoxisClient.onClose += () => { _heartbeatTimer.Stop(); };
        }

        /// <summary>
        /// Awake
        /// Called
        /// </summary>
        public void AwakeIns()
        {
            _protoQueue = new FiniteProcessQueue<HoxisProtocol>(protocolQueueCapacity, protocolQueueProcessQuantity);
            _protoQueue.onProcess += ProcessProtocol;
            _heartbeatTimer = new RegularTimer(heartbeatInterval, PostHeartbeat);
            isActive = true;
        }

        public void ResetIns(){ }

        void Update()
        {
            if (isActive)
            {
                // Get protocols from queue and process
                _protoQueue.ProcessInRound();
                // Update heartbeat timer
                _heartbeatTimer.Update(Time.deltaTime);
            } 
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
            _protoQueue.Enqueue(proto);
        }

        /// <summary>
        /// The poster of protocol
        /// Convert protocol to protocol bytes
        /// </summary>
        /// <param name="proto"></param>
        public void ProtocolPost(HoxisProtocol proto)
        {
            string json = FF.ObjectToJson(proto);
            byte[] data = FF.StringToBytes(json);
            HoxisClient.Send(data);
        }

        /// <summary>
        /// Rapidly send a request protocol
        /// </summary>
        /// <param name="method"></param>
        /// <param name="kvs"></param>
        public void Request(string method, params KVString[] kvs)
        {
            HoxisProtocol proto = new HoxisProtocol
            {
                type = ProtocolType.Request,
                handle = FF.ObjectToJson(new ReqHandle { req = method, ts = SF.GetTimeStamp(TimeUnit.Millisecond) }),
                err = "",
                receiver = HoxisProtocolReceiver.undef,
                sender = HoxisProtocolSender.undef,
                action = new HoxisProtocolAction(method, kvs),
                desc = ""
            };
            ProtocolPost(proto);
            // todo wait for response
        }

        public void Request(string method) { Request(method, null); }

        #region private functions

        private void ProcessProtocol(object state)
        {
            HoxisProtocol proto = (HoxisProtocol)state;
            switch (proto.type)
            {
                case ProtocolType.Response:
                    ReqHandle handle = FF.JsonToObject<ReqHandle>(proto.handle);
                    // todo 消除等待
                    if (proto.err != C.RESP_SUCCESS) { OnResponseError(proto.err, proto.desc); return; }
                    respCbTable[proto.action.method](proto.action.args);

                    break;
                case ProtocolType.Synchronization:
                    HoxisAgent agent = GetAgent(proto.sender.aid);
                    if (agent != null) agent.CallBehaviour(proto.action);
                    break;
                case ProtocolType.Proclamation:

                    break;
            }

        }
        private void PostHeartbeat() { Request("RefreshHeartbeat"); }
        private void OnResponseError(string err, string desc) { if (onResponseError == null) return; onResponseError(err, desc); }

        #endregion

        #region response callbacks
        private void QueryConnectionStateCb(HoxisProtocolArgs args)
        {
            string code = args["code"];
            Debug.Log("QueryConnectionStateCb code: " + code);
            if (code == C.RESP_SUCCESS)
                Debug.Log(args["state"]);
        }

        private void SignInCb(HoxisProtocolArgs args)
        {
            string code = args["code"];
            Debug.Log("SignInCb code: " + code);
        }

        private void SignOutCb(HoxisProtocolArgs args)
        {
            string code = args["code"];
            Debug.Log("SignOutCb code: " + code);
        }

        private void ReconnectCb(HoxisProtocolArgs args)
        {
            string code = args["code"];
            Debug.Log("ReconnectCb code: " + code);
        }

        private void RefreshHeartbeatCb(HoxisProtocolArgs args)
        {
            string code = args["code"];
            Debug.Log("RefreshHeartbeatCb code: " + code);
        }

        #endregion
    }
}