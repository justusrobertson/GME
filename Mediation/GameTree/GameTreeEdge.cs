using System;
using System.Text;

using Mediation.Interfaces;
using Mediation.PlanTools;

namespace Mediation.GameTree
{
    [Serializable]
    public class GameTreeEdge
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

        public GameTreeEdge () : this (new Operator(), -1, -1) { }

        public GameTreeEdge (Operator action, int parent) : this (action, parent, -1) { }

        public GameTreeEdge (Operator action, int parent, int child)
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
