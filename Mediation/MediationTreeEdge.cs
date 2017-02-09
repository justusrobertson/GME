using Mediation.Interfaces;
using Mediation.PlanTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mediation.MediationTree
{
    [Serializable]
    public class MediationTreeEdge
    {
        private Operator action;
        private int parent;
        private int child;

        // Access the edge's action.
        public IOperator Action
        {
            get { return action; }
            set { action = value as Operator; }
        }

        // Access the edge's outgoing node.
        public int Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        // Access the edge's incoming node.
        public int Child
        {
            get { return child; }
            set { child = value; }
        }

        public MediationTreeEdge() : this (new Operator(), -1, -1) { }

        public MediationTreeEdge(Operator action, int parent) : this (action, parent, -1) { }

        public MediationTreeEdge(Operator action, int parent, int child)
        {
            this.action = action as Operator;
            this.parent = parent;
            this.child = child;
        }

        // Displays the contents of the action.
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(Action);

            return sb.ToString();
        }
    }
}
