using System;
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
    /// -wrapping a protocol
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

        public static void ProtocolDataEntry(byte[] data)
        {
            string json = FormatFunc.BytesToString(data);
            Ret ret;
            HoxisProtocol proto = FormatFunc.JsonToObject<HoxisProtocol>(json, out ret);
            if (ret.code != 0)
                // todo LOG
                return;
            switch (proto.type) {
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

        public static void SynChannelEntry(HoxisProtocol proto)
        {
            HoxisID hid = proto.sndr.hid;
            Ret ret;
            HoxisAgent agent = GetAgent(hid, out ret);
            if (ret.code != 0)
                return;
            agent.CallBehaviour(proto.action);
        }

        public static void ReqChannelEntry(HoxisProtocol proto)
        {

        }

        public static void RespChannelEntry(HoxisProtocol proto)
        {

        }

        //具体调用方法的功能需使用链接的方式，解耦
    }
}