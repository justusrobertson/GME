using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Mediation.PlanTools;
using Mediation.Interfaces;

namespace Mediation.StateSpace
{
    public class StateSpaceNode
    {
        internal static int Counter = -1;

        public StateSpaceNode parent;
        public StateSpaceEdge incoming;
        public List<IOperator> systemActions;
        public State state;
        public List<StateSpaceEdge> outgoing;
        public Hashtable children;
        public Plan plan;
        public Problem problem;
        public Domain domain;
        public int id;
        public bool satisfiesGoal;

        public bool Goal
        {
            get
            {
                if (plan.Steps.Count == 0 && satisfiesGoal)
                    return true;

                return false;
            }
        }

        public bool Incompatible
        {
            get
            {
                if (plan.Steps.Count == 0 && !satisfiesGoal)
                    return true;

                return false;
            }
        }

        public StateSpaceNode ()
        {
            parent = null;
            incoming = null;
            systemActions = new List<IOperator>();
            state = new State();
            outgoing = new List<StateSpaceEdge>();
            children = new Hashtable();
            plan = new Plan();
            problem = new Problem();
            domain = new Domain();
            id = System.Threading.Interlocked.Increment(ref Counter);
            satisfiesGoal = false;
        }

        public StateSpaceNode (StateSpaceNode parent, StateSpaceEdge incoming, State state)
        {
            this.parent = parent;
            this.incoming = incoming;
            systemActions = new List<IOperator>();
            this.state = state;
            outgoing = new List<StateSpaceEdge>();
            children = new Hashtable();
            plan = new Plan();
            problem = new Problem();
            domain = new Domain();
            id = System.Threading.Interlocked.Increment(ref Counter);
            satisfiesGoal = false;
        }

        // Displays the contents of the action.
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(id);

            return sb.ToString();
        }
    }
}
