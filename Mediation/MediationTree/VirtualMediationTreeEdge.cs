using Mediation.Enums;
using Mediation.PlanTools;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mediation.MediationTree
{
    [Serializable]
    public class VirtualMediationTreeEdge : MediationTreeEdge
    {
        // Access the edge's action.
        public List<Operator> Actions { get; set; }

        public VirtualMediationTreeEdge() : this (new List<Operator>(), -1, -1) { }

        public VirtualMediationTreeEdge(List<Operator> actions, int parent) : this (actions, parent, -1) { }

        public VirtualMediationTreeEdge(List<Operator> actions, int parent, int child)
        {
            actionType = new ActionType();
            action = null;
            this.parent = parent;
            this.child = child;
            Actions = actions;
        }

        // Returns a hashcode.
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                // Suitable nullity checks etc, of course :)
                hash = hash * 23 + actionType.GetHashCode();
                hash = hash * 23 + parent.GetHashCode();
                hash = hash * 23 + child.GetHashCode();
                foreach (Operator act in Actions)
                    hash = hash * 23 + act.GetHashCode();

                return hash;
            }
        }

        // Checks if two predicates are equal.
        public override bool Equals(Object obj)
        {
            VirtualMediationTreeEdge edge = obj as VirtualMediationTreeEdge;

            if (edge.Actions.Count == Actions.Count)
            {
                foreach (Operator action in edge.Actions)
                    if (!Actions.Contains(action))
                        return false;

                foreach (Operator action in Actions)
                    if (!edge.Actions.Contains(action))
                        return false;

                if (parent != edge.Parent)
                    return false;

                return true;
            }

            return false;
        }

        // Displays the contents of the action.
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (Operator action in Actions) sb.Append(action);

            return sb.ToString();
        }
    }
}
