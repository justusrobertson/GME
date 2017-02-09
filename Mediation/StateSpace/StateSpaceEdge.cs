using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mediation.Enums;
using Mediation.Interfaces;
using Mediation.PlanTools;

using Mediation.Planners;

namespace Mediation.StateSpace
{ 
    public class StateSpaceEdge : IMediationEdge
    {
        private Operator action;
        private ActionType actionType;
        private List<IOperator> systemActions;

        public CausalLink clobberedLink;

        // Access the edge's action.
        public IOperator Action
        {
            get { return action; }
            set { action = value as Operator; }
        }

        // Access the edge's type.
        public ActionType ActionType
        {
            get { return actionType; }
            set { actionType = value; }
        }

        // Access the system actions.
        public List<IOperator> SystemActions
        {
            get { return systemActions; }
            set { systemActions = value; }
        }

        public StateSpaceEdge ()
        {
            action = new Operator();
            actionType = new ActionType();
            clobberedLink = new CausalLink();
        }

        public StateSpaceEdge(Operator action, ActionType actionType)
        {
            this.action = action as Operator;
            this.actionType = actionType;
            this.systemActions = new List<IOperator>();
        }

        // Displays the contents of the action.
        public override string ToString ()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(Action);

            return sb.ToString();
        }
    }
}
