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
        protected Dictionary<string, ActionHandler> behavTable = new Dictionary<string, ActionHandler>();

        /// <summary>
        /// Reflect an action to method with protocol argument form
        /// </summary>
        /// <param name="method"></param>
        /// <param name="args"></param>
        public void Act(string method, Dictionary<string, string> args)
        {
            if (!behavTable.ContainsKey(method))
                return;
            if (args == null) { behavTable[method](new Dictionary<string, string>()); }
            behavTable[method](args);
        }
        public void Act(string method) { Act(method, null); }
        public void Act(HoxisProtocolAction action) { Act(action.method, action.args); }
        public void Act(HoxisProtocol proto) { Act(proto.action.method, proto.action.args); }
    }
}

