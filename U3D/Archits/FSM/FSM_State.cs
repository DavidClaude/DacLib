using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DacLib.Generic;

namespace DacLib.U3D.Archits.FSM
{
    /// <summary>
    /// Base of states
    /// </summary>
    public abstract class StateNode : IFSMNodal
    {
        public event NoneForVoid_Handler onEnter, onUpdate, onExit;

        #region IFSMNodal
        public string name { get; protected set; }
        public NodeType nodeType { get; protected set; }
        public NodePort nodePort { get; set; }
        public void OnEnter() { if (onEnter == null) return; onEnter(); }
        public void OnUpdate() { if (onEnter == null) return; onUpdate(); }
        public void OnExit() { if (onEnter == null) return; onExit(); }
        #endregion
    }

    public class State : StateNode
    {
        public List<string> substateNames { get; }

        public State(string nameArg, NodePort nodePortArg = NodePort.None)
        {
            name = nameArg;
            nodeType = NodeType.Unit;
            nodePort = nodePortArg;
            substateNames = new List<string>();
        }

        /// <summary>
        /// Add a substate(name) to the substate list this state supports
        /// </summary>
        /// <param name="names"></param>
        public void AddSubstate(params string[] names)
        {
            foreach (string n in names)
            {
                if (ContainSubstate(n)) continue;
                substateNames.Add(n);
            }
        }

        /// <summary>
        /// If the substate list supports the given
        /// </summary>
        /// <param name="nameArg"></param>
        /// <returns></returns>
        public bool ContainSubstate(string nameArg)
        {
            foreach (string ssn in substateNames) { if (ssn == nameArg) return true; }
            return false;
        }
    }

    public class Substate : IEUE
    {
        public string name { get; private set; }
        public bool isActive { get; private set; }
        public event NoneForVoid_Handler onEnter, onUpdate, onExit;

        public Substate(string nameArg)
        {
            name = nameArg;
            isActive = false;
        }

        public void SetActive(bool active) { isActive = active; }

        #region IEUE
        public void OnEnter() { if (onEnter == null) return; onEnter(); }
        public void OnUpdate() { if (onEnter == null) return; onUpdate(); }
        public void OnExit() { if (onEnter == null) return; onExit(); }
        #endregion
    }
}
