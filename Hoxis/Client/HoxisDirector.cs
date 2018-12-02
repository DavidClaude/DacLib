﻿using System;
using System.Collections;
using System.Collections.Generic;
using DacLib.Generic;

namespace DacLib.Hoxis.Client
{
    /// <summary>
    /// Manager of all HoxisAgents in current cluster
    /// Major in:
    /// -manage agents
    /// -search for a given agent or its gameObject
    /// -parse protocols from HoxisClient, then send them to correct agents
    /// -wrapp and launch a protocol
    /// -etc.
    /// </summary>
    public static class HoxisDirector
    {
        #region ret codes
        public const byte RET_HID_EXISTS = 1;
        public const byte RET_NO_UNOCCUPIED_HID = 2;
        public const byte RET_NO_HID = 3;
        #endregion

        private static Dictionary<HoxisID, HoxisAgent> _agentSearcher = new Dictionary<HoxisID, HoxisAgent>();

        public static void Init(string clientConfigPath)
        {
            HoxisClient.InitConfig(clientConfigPath);
            HoxisClient.onExtract += ProtocolEntry;
        }

        /// <summary>
        /// Add an agent to searcher
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="ret"></param>
        public static void Register(HoxisAgent agent, out Ret ret)
        {
            HoxisID hid = agent.hoxisID;
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
        public static void Remove(HoxisID id, out Ret ret)
        {
            if (!_agentSearcher.ContainsKey(id))
            {
                ret = new Ret(LogLevel.Warning, RET_NO_HID, "HoxisID:" + id.group + "," + id.id + " doesn't exist");
                return;
            }
            _agentSearcher.Remove(id);
            ret = Ret.ok;
        }

        public static void Remove(HoxisAgent agent, out Ret ret)
        {
            HoxisID hid = agent.hoxisID;
            Remove(hid, out ret);
        }

        /// <summary>
        /// Get an unoccupied id of given group
        /// Generally used when a GameObject with HoxisAgent instantiated
        /// </summary>
        /// <param name="group"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public static ushort GetUnoccupiedID(string group, out Ret ret)
        {
            int len = _agentSearcher.Count;
            for (ushort i = 0; i <= ushort.MaxValue; i++)
            {
                HoxisID hid = new HoxisID(group, i);
                if (_agentSearcher.ContainsKey(hid))
                    continue;
                ret = Ret.ok;
                return i;
            }
            ret = new Ret(LogLevel.Warning, RET_NO_UNOCCUPIED_HID, "Can't figure out occupied id, there must be an error");
            return 0;
        }

        /// <summary>
        /// Search for an agent through searcher
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public static HoxisAgent GetAgent(HoxisID id, out Ret ret)
        {
            if (!_agentSearcher.ContainsKey(id))
            {
                ret = new Ret(LogLevel.Warning, RET_NO_HID, "HoxisID:" + id.group + "," + id.id + " doesn't exist");
                return null;
            }
            ret = Ret.ok;
            return _agentSearcher[id];
        }

        public static HoxisAgent GetAgent(HoxisID id)
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
        public static void ProtocolEntry(byte[] data)
        {
            string json = FormatFunc.BytesToString(data);
            UnityEngine.Debug.Log(json);
            Ret ret;
            HoxisProtocol proto = FormatFunc.JsonToObject<HoxisProtocol>(json, out ret);
            if (ret.code != 0)
            {
                // todo LOG
                UnityEngine.Debug.Log(ret.desc);
                return;
            }
            switch (proto.type)
            {
                case ProtocolType.Synchronization:
                    SynChannelEntry(proto);
                    break;
                case ProtocolType.Request:
                    ReqChannelEntry(proto);
                    break;
                case ProtocolType.Response:
                    RespChannelEntry(proto);
                    break;
            }
        }

        /// <summary>
        /// The poster of protocol
        /// Convert protocol to protocol bytes
        /// </summary>
        /// <param name="proto"></param>
        public static void ProtocolPost(HoxisProtocol proto)
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
            string json = FormatFunc.ObjectToJson(proto);
            byte[] data = FormatFunc.StringToBytes(json);
            HoxisClient.BeginSend(data);
        }

        #region private functions

        /// <summary>
        /// **WITHIN THREAD**
        /// The entrance of synchronization protocol
        /// </summary>
        /// <param name="proto"></param>
        private static void SynChannelEntry(HoxisProtocol proto)
        {
            HoxisID hid = proto.sndr.hid;
            HoxisAgent agent = GetAgent(hid);            
            agent.Implement(proto.action);
        }

        /// <summary>
        /// **WITHIN THREAD**
        /// The entrance of request protocol
        /// </summary>
        /// <param name="proto"></param>
        private static void ReqChannelEntry(HoxisProtocol proto)
        {
            // todo
        }

        /// <summary>
        /// **WITHIN THREAD**
        /// The entrance of response protocol
        /// Elimate the waiting state of request in reception
        /// </summary>
        /// <param name="proto"></param>
        private static void RespChannelEntry(HoxisProtocol proto)
        {

        }

        #endregion
    }
}