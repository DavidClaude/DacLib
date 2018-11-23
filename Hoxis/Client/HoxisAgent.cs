using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DacLib.Generic;

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
        public HoxisType hoxisType { get; private set; }

        /// <summary>
        /// The unique id for searching
        /// </summary>
        /// <value>The hoxis identifier.</value>
        public HoxisID hoxisID { get; private set; }

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
        private Queue<HoxisProtocolAction> _actionQueue;
        private short _processingQuantity;

        // Use this for initialization
        void Start()
        {
            // Init the action queue by HoxisClient.config
            int capacity;
            Ret retCapacity;
            capacity = HoxisClient.config.GetInt("protocol", "action_queue_capacity", out retCapacity);
            if (retCapacity.code != 0) { capacity = 32; }
            _actionQueue = new Queue<HoxisProtocolAction>(capacity);

            // Init the processing quantity by HoxisClient.config
            Ret retQuantity;
            _processingQuantity = HoxisClient.config.GetShort("protocol", "processing_quantity", out retQuantity);
            if (retQuantity.code != 0) { _processingQuantity = 5; }
        }

        /// <summary>
        /// Construct function for MonoBehaviour
        /// </summary>
        /// <param name="hoxisTypeArg"></param>
        /// <param name="hoxisIDArg"></param>
        /// <param name="autoSynArg"></param>
        public void CoFunc(HoxisType hoxisTypeArg, HoxisID hoxisIDArg, bool autoSynArg = true)
        {
            hoxisType = hoxisTypeArg;
            hoxisID = hoxisIDArg;
            autoSyn = autoSynArg;
            isPlayer = (hoxisType == HoxisType.Host ? true : false);
            _behav = GetComponent<HoxisBehaviour>();
        }

        void Update()
        {
            // Process actions
            short c = 0;
            while (c < _processingQuantity)
            {
                if (_actionQueue.Count <= 0)
                    break;
                HoxisProtocolAction action = _actionQueue.Dequeue();
                CallBehaviour(action);
                c++;
            }

            // todo autosyn, distance detection
        }

        /// <summary>
        /// **WITHIN THREAD**
        /// Push an action into the queue
        /// </summary>
        /// <param name="action"></param>
        public void Push(HoxisProtocolAction action)
        {
            lock (_actionQueue) { _actionQueue.Enqueue(action); }
        }
        public void Push(HoxisProtocol proto) { Push(proto.action); }

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
                rcvr = new HoxisProtocolReceiver
                {
                    type = ReceiverType.Cluster,
                    hid = HoxisID.nil,
                },
                sndr = new HoxisProtocolSender
                {
                    hid = hoxisID,
                    loopback = true,
                },
                action = actionArg,
                desc = "",
            };
            HoxisDirector.ProtocolPost(proto);
        }

        public void Report(string methodArg, params KV<string, string>[] kvs)
        {
            Dictionary<string, string> argsArg = new Dictionary<string, string>();
            foreach (KV<string, string> kv in kvs) { argsArg.Add(kv.key, kv.val); }
            HoxisProtocolAction action = new HoxisProtocolAction
            {
                method = methodArg,
                args = argsArg,
            };
            Report(action);
        }

        public void Report(string methodArg, Dictionary<string, string> argsArg)
        {
            HoxisProtocolAction action = new HoxisProtocolAction
            {
                method = methodArg,
                args = argsArg,
            };
            Report(action);
        }

        public void Report(HoxisProtocol proto)
        {
            HoxisDirector.ProtocolPost(proto);
        }

        /// <summary>
        /// Call the behaviour-layer
        /// </summary>
        /// <param name="action"></param>
        private void CallBehaviour(HoxisProtocolAction action) { _behav.Act(action); }
        private void CallBehaviour(HoxisProtocol proto) { _behav.Act(proto); }


    }
}