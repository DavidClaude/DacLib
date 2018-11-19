using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DacLib.Hoxis.Client
{
    public class HoxisBehaviour : MonoBehaviour
    {
        protected static Dictionary<string, ProtocolHandler> behavTable = new Dictionary<string, ProtocolHandler>();
    }
}

