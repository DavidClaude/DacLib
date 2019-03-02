using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DacLib.Generic;

namespace DacLib.U3D.Archits.FSM
{
    public class Transition
    {
        /// <summary>
        /// From what Node
        /// </summary>
        public IFSMNodal fromNode { get; }

        /// <summary>
        /// To what state
        /// </summary>
        public IFSMNodal toNode { get; }

        /// <summary>
        /// Events of begin and end
        /// </summary>
        public event NoneForVoid_Handler onBegin, onEnd;

        public Transition(IFSMNodal fromArg, IFSMNodal toArg)
        {
            fromNode = fromArg;
            toNode = toArg;
        }

        public void OnBegin() { if (onBegin == null) return; onBegin(); }
        public void OnEnd() { if (onBegin == null) return; onEnd(); }
    }
}
