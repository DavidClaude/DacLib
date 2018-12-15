using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DacLib.Generic;
using DacLib.Codex;

namespace DacLib.Hoxis.Client
{

    /// <summary>
    /// The unique ScriptComponent of protocol-layer
    /// Major in:
    /// -Receive actions and reflect to behaviours
    /// -Called by control-layer to upgrade actions
    /// -Maintain a action queue and limit the processing quantity per frame
    /// -Auto synchronize transforms, states regularly
    /// -If player, manage the proxy and perpetual agents by Distance-detection, request or release dealership
    /// </summary>
    [RequireComponent(typeof(HoxisBehaviour))]
    public class HoxisAgent : MonoBehaviour
    {
        /// <summary>
        /// Host, Proxy, Perpetual or None ?
        /// </summary>
        public AgentType hoxisType { get; private set; }

        /// <summary>
        /// The unique id for searching
        /// </summary>
        /// <value>The hoxis identifier.</value>
        public HoxisAgentID id { get; private set; }

        /// <summary>
        /// Auto synchronized or not
        /// </summary>
        /// <value><c>true</c> if auto syn; otherwise, <c>false</c>.</value>
        public bool autoSyn { get; set; }

        /// <summary>
        /// Is this gameObject player ?
        /// </summary>
        public bool isPlayer { get; private set; }

        private HoxisBehaviour _behav;

        // Use this for initialization
        void Start()
        {

        }

        /// <summary>
        /// Construct function for MonoBehaviour
        /// </summary>
        /// <param name="hoxisTypeArg"></param>
        /// <param name="hoxisIDArg"></param>
        /// <param name="autoSynArg"></param>
        public void Initialize(AgentType agentTypeArg, HoxisAgentID idArg, bool autoSynArg = true)
        {
            hoxisType = agentTypeArg;
            id = idArg;
            autoSyn = autoSynArg;
            isPlayer = (hoxisType == AgentType.Host ? true : false);
            _behav = GetComponent<HoxisBehaviour>();
        }

        void Update()
        {
            // todo autosyn, distance detection
        }

        /// <summary>
        /// Call the behaviour-layer
        /// </summary>
        /// <param name="action"></param>
        public void CallBehaviour(HoxisProtocolAction action) { _behav.Act(action); }
        public void CallBehaviour(HoxisProtocol proto) { _behav.Act(proto); }
        public void CallBehaviour(object state) { _behav.Act((HoxisProtocolAction)state); }

        /// <summary>
        /// Report an action of this gameObject, generally syn
        /// </summary>
        /// <param name="action"></param>
        public void Report(HoxisProtocolAction actionArg)
        {
            HoxisProtocol proto = new HoxisProtocol
            {
                type = ProtocolType.Synchronization,
                handle = "",
                receiver = HoxisProtocolReceiver.cluster,
                sender = new HoxisProtocolSender
                {
                    aid = id,
                    loopback = true,
                },
                action = actionArg,
                desc = "",
            };
            Report(proto);
        }

        public void Report(string methodArg, params KVString[] kvs)
        {
            HoxisProtocolAction action = new HoxisProtocolAction
            {
                method = methodArg,
                args = new HoxisProtocolArgs(kvs),
            };
            Report(action);
        }

        public void Report(string methodArg, Dictionary<string, string> argsArg)
        {
            HoxisProtocolAction action = new HoxisProtocolAction
            {
                method = methodArg,
                args = new HoxisProtocolArgs { values = argsArg},
            };
            Report(action);
        }

        /// <summary>
        /// Build custom protocol to report
        /// </summary>
        /// <param name="proto"></param>
        public void Report(HoxisProtocol proto) { HoxisDirector.Ins.ProtocolPost(proto); }
    }
}