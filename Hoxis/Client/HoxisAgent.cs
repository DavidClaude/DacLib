using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DacLib.Hoxis.Client
{

    /// <summary>
    /// The unique ScriptComponent of Protocol layer
    /// Major in:
    /// -Receive actions and reflect to behaviours
    /// -Called by Controller layer to upgrade actions
    /// -Auto synchronize transforms, states regularly
    /// -If player, manage the proxy and perpetual agents by Distance-detection, request or release dealership
    /// </summary>
    [RequireComponent(typeof(HoxisBehaviour))]
    public class HoxisAgent : MonoBehaviour
	{
   		/// <summary>
		/// Host, Proxy, Perpetual or None ?
		/// </summary>
		public HoxisType hoxisType {get; private set;}

		/// <summary>
		/// The unique id for searching
		/// </summary>
		/// <value>The hoxis identifier.</value>
		public HoxisID hoxisID { get; private set;}

		/// <summary>
		/// Auto synchronized or not
		/// </summary>
		/// <value><c>true</c> if auto syn; otherwise, <c>false</c>.</value>
		public bool autoSyn { get; set;}

        /// <summary>
        /// Is this gameObject player ?
        /// </summary>
        public bool isPlayer { get; private set; }

        private HoxisBehaviour _behav;

        // Use this for initialization
        void Start ()
		{
			
		}

        /// <summary>
        /// Construct function for MonoBehaviour
        /// </summary>
        /// <param name="hoxisTypeArg"></param>
        /// <param name="hoxisIDArg"></param>
        /// <param name="autoSynArg"></param>
        public void CoFunc (HoxisType hoxisTypeArg, HoxisID hoxisIDArg, bool autoSynArg = true)
		{
            hoxisType = hoxisTypeArg;
			hoxisID = hoxisIDArg;
			autoSyn = autoSynArg;
            isPlayer = (hoxisType == HoxisType.Host ? true : false);
            _behav = GetComponent<HoxisBehaviour>();
        }

		void Update ()
		{
		    // todo autosyn, distance detection
		}

        public void CallBehaviour(HoxisProtocolAction action) { _behav.Act(action); }
        public void CallBehaviour(HoxisProtocol proto) { _behav.Act(proto); }


        /// <summary>
        /// Trigger the behaviour
        /// Called by HoxisAgent
        /// </summary>
        /// <param name="method"></param>
        /// <param name="proto"></param>
        //public void BehavTrigger(string method, Dictionary<string, string> args)
        //{
        //    if (!.ContainsKey(method))
        //        return;
        //    if (args == null)
        //    {
        //        behavTable[method](new Dictionary<string, string>());
        //    }
        //    behavTable[method](args);
        //}
        //public void BehavTrigger(HoxisProtocol proto)
        //{
        //    BehavTrigger(proto.action.method, proto.action.args);
        //}
        //public void BehavTrigger(HoxisProtocolAction action)
        //{
        //    BehavTrigger(action.method, action.args);
        //}
        //public void BehavTrigger(string method)
        //{
        //    BehavTrigger(method, null);
        //}
    }
}