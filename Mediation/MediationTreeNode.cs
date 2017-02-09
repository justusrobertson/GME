using Mediation.FileIO;
using Mediation.PlanTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mediation.MediationTree
{
    [Serializable]
    public class MediationTreeNode
    {
        private MediationTreeEdge incoming;
        private State state;
        private Plan plan;
        private List<MediationTreeEdge> outgoing;
        private Problem problem;
        private Domain domain;
        private int id;

        private bool satisfiesGoal;
        private bool deadEnd;

        private int depth;

        public State State
        {
            get { return state; }
            set { state = value; }
        }

        public Plan Plan
        {
            get { return plan; }
            set { plan = value; }
        }

        public Domain Domain
        {
            get { return domain; }
        }

        public Problem Problem
        {
            get { return problem; }
        }

        public int Depth
        {
            get { return depth; }
        }

        public List<MediationTreeEdge> Outgoing
        {
            get { return outgoing; }
            set { outgoing = value; }
        }

        public MediationTreeEdge Incoming
        {
            get { return incoming; }
        }

        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        public bool IsGoal
        {
            get { return satisfiesGoal; }
        }

        public bool DeadEnd
        {
            get { return deadEnd; }
        }

        public MediationTreeNode(Domain domain, Problem problem, Plan plan, int id) : this (domain, problem, null, new State(problem.Initial, null, null), plan, id, 0) { }

        public MediationTreeNode(Domain domain, Problem problem, State state, Plan plan, int id) : this (domain, problem, null, state, plan, id, 0) { }

        public MediationTreeNode(Domain domain, Problem problem, MediationTreeEdge incoming, State state, Plan plan, int id, int depth)
        {
            this.domain = domain;
            this.problem = problem;
            this.incoming = incoming;
            this.id = id;
            this.state = state;
            this.depth = depth;

            outgoing = new List<MediationTreeEdge>();

            if (state.Satisfies(problem.Goal)) satisfiesGoal = true;
            else satisfiesGoal = false;

            deadEnd = false;
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
