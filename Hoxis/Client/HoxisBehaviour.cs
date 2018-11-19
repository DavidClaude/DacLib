using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DacLib.Hoxis.Client
{
    /// <summary>
    /// Base class of Behaviour layer
    /// Not allowed to be used immediately as ScriptComponent
    /// </summary>
    public class HoxisBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Action reflection table
        /// Be shared by all HoxisBehaviour instances, derived classes of this class must register their own methods for reflection
        /// </summary>
        protected static Dictionary<string, ProtocolHandler> behavTable = new Dictionary<string, ProtocolHandler>();

        /// <summary>
        /// Trigger the behaviour
        /// Called by HoxisAgent
        /// </summary>
        /// <param name="method"></param>
        /// <param name="proto"></param>
        public void BehavTrigger(string method, Dictionary<string, string> args)
        {
            if (!behavTable.ContainsKey(method))
                return;
            if (args == null) {
                behavTable[method](new Dictionary<string, string>());
            }
            behavTable[method](args);
        }
        public void BehavTrigger(HoxisProtocol proto)
        {
            BehavTrigger(proto.action.mthd, proto.action.args);
        }
        public void BehavTrigger(string method)
        {
            BehavTrigger(method, null);
        }
    }
}

