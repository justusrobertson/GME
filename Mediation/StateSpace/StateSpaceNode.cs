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
        private static int Counter = -1;

        public StateSpaceNode parent;
        public StateSpaceEdge incoming;
        public List<IOperator> systemActions;
        public State state;
        public List<StateSpaceEdge> outgoing;
        public Hashtable children;
        public Plan plan;
        public Problem problem;
        public int id;

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
            id = System.Threading.Interlocked.Increment(ref Counter);
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
            id = System.Threading.Interlocked.Increment(ref Counter);
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
