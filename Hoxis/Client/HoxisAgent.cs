using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DacLib.Hoxis.Client
{

    /// <summary>
    /// The unique ScriptComponent of protocol-layer
    /// Major in:
    /// -Receive actions and reflect to behaviours
    /// -Called by control-layer to upgrade actions
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

        /// <summary>
        /// Call the behaviour-layer
        /// </summary>
        /// <param name="action"></param>
        public void CallBehaviour(HoxisProtocolAction action) { _behav.Act(action); }
        public void CallBehaviour(HoxisProtocol proto) { _behav.Act(proto); }
    }
}