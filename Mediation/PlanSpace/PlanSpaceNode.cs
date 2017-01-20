using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Mediation.PlanTools;


namespace Mediation.PlanSpace
{
    public class PlanSpaceNode
    {
        private static int Counter = -1;

        public PlanSpaceNode parent;
        public PlanSpaceEdge incoming;
        public List<PlanSpaceEdge> outgoing;
        public Hashtable children;
        public Plan plan;
        public Problem problem;
        public int id;

        public PlanSpaceNode ()
        {
            parent = null;
            incoming = null;
            outgoing = new List<PlanSpaceEdge>();
            children = new Hashtable();
            plan = new Plan();
            problem = new Problem();
            id = System.Threading.Interlocked.Increment(ref Counter);
        }

        public PlanSpaceNode(PlanSpaceNode parent, PlanSpaceEdge incoming)
        {
            this.parent = parent;
            this.incoming = incoming;
            outgoing = new List<PlanSpaceEdge>();
            children = new Hashtable();
            plan = new Plan();
            problem = new Problem();
            id = System.Threading.Interlocked.Increment(ref Counter);
        }
    }
}
