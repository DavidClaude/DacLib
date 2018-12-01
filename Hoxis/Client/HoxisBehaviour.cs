using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DacLib.Hoxis.Client
{
    /// <summary>
    /// Base class of behaviour-layer
    /// Not allowed to be used immediately as ScriptComponent
    /// </summary>
    public class HoxisBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Action reflected table
        /// Derived classes of this class must register for their own methods
        /// </summary>
        protected Dictionary<string, ActionArgsHandler> behavTable = new Dictionary<string, ActionArgsHandler>();

        /// <summary>
        /// Reflect an action to method with protocol argument form
        /// </summary>
        /// <param name="method"></param>
        /// <param name="args"></param>
        public void Act(string method, HoxisProtocolArgs args)
        {
            if (!behavTable.ContainsKey(method)) return;
            behavTable[method](args);
        }
        public void Act(string method) { Act(method, HoxisProtocolArgs.undef); }
        public void Act(HoxisProtocolAction action) { Act(action.method, action.args); }
        public void Act(HoxisProtocol proto) { Act(proto.action.method, proto.action.args); }
    }
}

