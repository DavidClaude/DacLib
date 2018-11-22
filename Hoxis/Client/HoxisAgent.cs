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
        private Queue<HoxisProtocolAction> _actionQueue = new Queue<HoxisProtocolAction>(Configs.ACTION_QUEUE_CAPACITY);

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
            while (c < Configs.MAX_PROCESSING_QUANTITY_PER_FRAME)
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
        /// Push an action to the queue
        /// </summary>
        /// <param name="action"></param>
        public void Push(HoxisProtocolAction action)
        {
            lock (_actionQueue) { _actionQueue.Enqueue(action); }
        }
        public void Push(HoxisProtocol proto) { Push(proto.action); }

        public void Report(HoxisProtocolAction action)
        {
            //HoxisDirector
        }

        public void Report(string method, params KV<string, string>[] kvs)
        {

        }

        public void Report(string method, Dictionary<string, string> args)
        {

        }

        /// <summary>
        /// Call the behaviour-layer
        /// </summary>
        /// <param name="action"></param>
        private void CallBehaviour(HoxisProtocolAction action) { _behav.Act(action); }
        private void CallBehaviour(HoxisProtocol proto) { _behav.Act(proto); }

        
    }
}