using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DacLib.Generic;
using FF = DacLib.Generic.FormatFunc;

namespace DacLib.U3D.Archits.FSM
{
    public class Diagram : IFSMNodal
    {
        public IFSMNodal this[string nodeName]
        {
            get
            {
                foreach (IFSMNodal n in nodes) { if (n.name == nodeName) return n; }
                return null;
            }
        }

        public IFSMNodal entrance
        {
            get
            {
                foreach (IFSMNodal n in nodes) { if (n.nodePort == NodePort.Entrance || n.nodePort == NodePort.Both) return n; }
                return null;
            }
        }

        public List<IFSMNodal> exitusList
        {
            get
            {
                List<IFSMNodal> list = new List<IFSMNodal>();
                foreach (IFSMNodal n in nodes) { if (n.nodePort == NodePort.Exitus || n.nodePort == NodePort.Both) list.Add(n); }
                return list;
            }
        }

        public IFSMNodal currentNode { get; private set; }

        public List<IFSMNodal> nodes { get; }
        public List<Substate> substates { get; }
        public List<Transition> transitions { get; }

        public Diagram(string nameArg, NodePort nodePortArg = NodePort.None)
        {
            name = nameArg;
            nodeType = NodeType.Diagram;
            nodePort = nodePortArg;
            nodes = new List<IFSMNodal>();
            substates = new List<Substate>();
            transitions = new List<Transition>();
        }

        public void GoTo(string nodeName)
        {
            // Does the target node exist ?
            IFSMNodal tar_node = this[nodeName];
            if (tar_node == null) return;

            // Does the transition between current and target exist ?
            Transition transition = GetTransition(currentNode, tar_node);
            if (transition == null) return;

            // If diagram, is its current node a exitus ?
            if (currentNode.nodeType == NodeType.Diagram)
            {
                Diagram diagram = (Diagram)currentNode;
                if (diagram.currentNode.nodePort != NodePort.Exitus && diagram.currentNode.nodePort != NodePort.Both) return;
            }

            // If state, deactivate substates which aren't supported by target state
            if (currentNode.nodeType == NodeType.Unit)
            {
                State tar_state = GetState(tar_node);
                State cur_state = GetState(currentNode);
                foreach (string ssn in cur_state.substateNames) { if (!tar_state.substateNames.Contains(ssn)) GetSubstate(ssn).SetActive(false); }
            }

            transition.fromNode.OnExit();
            transition.OnBegin();
            currentNode = tar_node;
            transition.OnEnd();
            transition.toNode.OnEnter();
        }

        #region GET

        public IFSMNodal GetNode(string nodeName)
        {
            foreach (IFSMNodal n in nodes) { if (n.name == nodeName) return n; }
            return null;
        }

        /// <summary>
        /// If the node list contains one with given name
        /// </summary>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public bool ContainNodeName(string nodeName)
        {
            foreach (IFSMNodal n in nodes) { if (n.name == nodeName) return true; }
            return false;
        }
        public State GetState(string nodeName) { return GetState(this[nodeName]); }
        public State GetState(IFSMNodal node) { return (State)node; }
        public Diagram GetDiagram(string nodeName) { return GetDiagram(this[nodeName]); }
        public Diagram GetDiagram(IFSMNodal node) { return (Diagram)node; }
        public Substate GetSubstate(string subName)
        {
            foreach (Substate ss in substates) { if (ss.name == subName) return ss; }
            return null;
        }
        public bool ContainSubstateName(string subName)
        {
            foreach (Substate ss in substates) { if (ss.name == subName) return true; }
            return false;
        }
        public Transition GetTransition(string fromName, string toName)
        {
            foreach (Transition t in transitions) { if (t.fromNode.name == fromName && t.toNode.name == toName) return t; }
            return null;
        }
        public Transition GetTransition(IFSMNodal fromNode, IFSMNodal toNode)
        {
            foreach (Transition t in transitions) { if (t.fromNode == fromNode && t.toNode == toNode) return t; }
            return null;
        }

        public bool ContainTransitionName(string fromName, string toName)
        {
            foreach (Transition t in transitions) { if (t.fromNode.name == fromName && t.toNode.name == toName) return true; }
            return false;
        }

        #endregion

        #region CREATE

        public IFSMNodal CreateNode(string nameArg, NodeType typeArg = NodeType.Unit, NodePort portArg = NodePort.None)
        {
            if (ContainNodeName(nameArg)) return null;
            IFSMNodal node = null;
            switch (typeArg)
            {
                case NodeType.Unit:
                    node = new State(nameArg, portArg);
                    break;
                case NodeType.Diagram:
                    node = new Diagram(nameArg, portArg);
                    break;
            }
            nodes.Add(node);
            return node;
        }
        public void CreateNode(params string[] nameArgs) { foreach (string n in nameArgs) CreateNode(n); }
        public State CreateState(string nameArg, NodePort portArg = NodePort.None) { return (State)CreateNode(nameArg, NodeType.Unit, portArg); }
        public Diagram CreateDiagram(string nameArg, NodePort portArg = NodePort.None) { return (Diagram)CreateNode(nameArg, NodeType.Diagram, portArg); }


        /// <summary>
        /// Quickly create nodes and transition
        /// </summary>
        /// <param name="fromName"></param>
        /// <param name="toName"></param>
        /// <returns></returns>
        public Transition QuickCreate(string fromName, string toName)
        {
            IFSMNodal fromNode = this[fromName];
            if (fromNode == null) fromNode = CreateNode(fromName);
            IFSMNodal toNode = this[toName];
            if (toNode == null) toNode = CreateNode(toName);
            if (ContainTransitionName(fromName, toName)) return null;
            Transition transition = new Transition(fromNode, toNode);
            transitions.Add(transition);
            return transition;
        }

        /// <summary>
        /// Quickly create nodes and bidirectional transition
        /// </summary>
        /// <param name="fromName"></param>
        /// <param name="toName"></param>
        public void QuickCreateBidirection(string fromName, string toName)
        {
            IFSMNodal fromNode = this[fromName];
            if (fromNode == null) fromNode = CreateNode(fromName);
            IFSMNodal toNode = this[toName];
            if (toNode == null) toNode = CreateNode(toName);
            if (ContainTransitionName(fromName, toName)) return;
            Transition transition = new Transition(fromNode, toNode);
            transitions.Add(transition);
            if (ContainTransitionName(toName, fromName)) return;
            Transition transition_reverse = new Transition(toNode, fromNode);
            transitions.Add(transition_reverse);
        }

        public void Connect(IFSMNodal fromNode, IFSMNodal toNode, bool bidirectional = false)
        {
            if (!ContainNodeName(fromNode.name)) nodes.Add(fromNode);
            if (!ContainNodeName(toNode.name)) nodes.Add(toNode);
            if (bidirectional) QuickCreateBidirection(fromNode.name, toNode.name);
            else { QuickCreate(fromNode.name, toNode.name); }
        }

        public Substate CreateSubstate(string nameArg)
        {
            if (ContainSubstateName(nameArg)) return null;
            Substate sub = new Substate(nameArg);
            substates.Add(sub);
            return sub;
        }

        public void QuickBindSubstate(string stateName, params string[] substateNames)
        {
            State state = GetState(stateName);
            if (state == null) return;
            foreach (string ssn in substateNames)
            {
                Substate sub = GetSubstate(ssn);
                if (sub == null) sub = CreateSubstate(ssn);
                state.AddSubstate(sub.name);
            }
        }

        #endregion

        #region OPERATION

        public void SetNodePort(string nodeName, NodePort port)
        {
            IFSMNodal node = this[nodeName];
            if (node == null) return;
            node.nodePort = port;
        }

        public string LogAll()
        {
            string log = "";

            foreach (IFSMNodal n in nodes)
            {
                log += "--State--\n";
                log += "\tName: " + n.name + "\n";
                log += "\tType: " + n.nodeType + "\n";
                log += "\tPort: " + n.nodePort + "\n";
                if (n.nodeType == NodeType.Unit)
                {
                    log += "\tSubstates: \n";
                    foreach (string ssn in GetState(n).substateNames)
                    {
                        log += "\t\t" + ssn + "\n";
                    }
                }

            }

            foreach (Transition t in transitions)
            {
                log += "--Transition--\n";
                log += "\tBegin: " + t.fromNode.name + "\n";
                log += "\tEnd: " + t.toNode.name + "\n";
            }

            foreach (Substate ss in substates)
            {
                log += "--Substate--\n";
                log += "\tName: " + ss.name + "\n";
                log += "\tIsActive: " + ss.isActive + "\n";
            }
            return log;
        }

        #endregion

        #region IFSMNodal
        public string name { get; }
        public NodeType nodeType { get; }
        public NodePort nodePort { get; set; }

        public void OnEnter()
        {
            if (entrance == null) return;
            currentNode = entrance;
            currentNode.OnEnter();
        }

        public void OnUpdate()
        {
            if (currentNode == null) return;
            currentNode.OnUpdate();

            // Keep all active substates running
            if (currentNode.nodeType == NodeType.Unit) { foreach (Substate ss in substates) { if (ss.isActive) ss.OnUpdate(); } }
        }

        public void OnExit()
        {
            currentNode.OnExit();
            currentNode = null;
        }
        #endregion
    }
}
