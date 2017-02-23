using Mediation.Interfaces;
using Mediation.PlanTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mediation.Enums;

namespace Mediation.MediationTree
{
    [Serializable]
    public class MediationTreeEdge
    {
        protected ActionType actionType;
        public ActionType ActionType
        {
            get { return actionType; }
            set { actionType = value; }
        }

        protected Operator action;
        // Access the edge's action.
        public IOperator Action
        {
            get { return action; }
            set { action = value as Operator; }
        }

        protected int parent;
        // Access the edge's outgoing node.
        public int Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        protected int child;
        // Access the edge's incoming node.
        public int Child
        {
            get { return child; }
            set { child = value; }
        }

        public MediationTreeEdge() : this (new Operator(), new ActionType(), -1, -1) { }

        public MediationTreeEdge(Operator action, ActionType actionType, int parent) : this (action, actionType, parent, -1) { }

        public MediationTreeEdge(Operator action, ActionType actionType, int parent, int child)
        {
            this.actionType = actionType;
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
