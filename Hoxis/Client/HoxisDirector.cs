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
        public const ushort RET_HID_EXISTS = 1;
        public const ushort RET_NO_UNOCCUPIED_HID = 2;
        public const ushort RET_NO_HID = 3;

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
        public static void Repeal(HoxisID id, out Ret ret)
        {
            if (!_agentSearcher.ContainsKey(id))
            {
                ret = new Ret(LogLevel.Warning, RET_NO_HID, "HoxisID:" + id.group + "," + id.id + " doesn't exist");
                return;
            }
            _agentSearcher.Remove(id);
            ret = Ret.ok;
        }

        public static void Repeal(HoxisAgent agent, out Ret ret)
        {
            HoxisID hid = agent.hoxisID;
            Repeal(hid, out ret);
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
            for (ushort i = 0; i < 65535; i++)
            {
                HoxisID hid = new HoxisID(group, i);
                if (_agentSearcher.ContainsKey(hid))
                    continue;
                ret = Ret.ok;
                return i;
            }
            ret = new Ret(LogLevel.Warning, RET_NO_UNOCCUPIED_HID, "No occupied id, there must be an error");
            return 0;
        }

        /// <summary>
        /// Search for an agent through searcher
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public static HoxisAgent Search(HoxisID id,out Ret ret)
        {
            if (!_agentSearcher.ContainsKey(id)) {
                ret = new Ret(LogLevel.Warning, RET_NO_HID, "HoxisID:" + id.group + "," + id.id + " doesn't exist");
                return null;
            }
            ret = Ret.ok;
            return _agentSearcher[id];
        }

        //具体调用方法的功能需使用链接的方式，解耦
    }
}