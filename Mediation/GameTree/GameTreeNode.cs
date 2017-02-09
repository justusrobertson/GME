using System;
using System.Collections.Generic;
using System.Text;

using Mediation.PlanTools;
using Mediation.FileIO;

namespace Mediation.GameTree
{
    [Serializable]
    public class GameTreeNode
    {
        private GameTreeEdge incoming;
        private State state;
        private List<GameTreeEdge> outgoing;
        private List<GameTreeEdge> unplayed;
        private Problem problem;
        private Domain domain;
        private int id;

        private bool satisfiesGoal;
        private bool deadEnd;

        private int timesPlayed;
        private int wins;

        private int depth;

        // Tracks the total number of times the node has been played.
        public int TimesPlayed
        {
            get { return timesPlayed; }
        }

        // Tracks the total number of wins the node has recorded.
        public int Wins
        {
            get { return wins; }
            set { wins = value; }
        }

        public State State
        {
            get { return state; }
            set { state = value; }
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

        public List<GameTreeEdge> Outgoing
        {
            get { return outgoing; }
            set { outgoing = value; }
        }

        public List<GameTreeEdge> Unplayed
        {
            get { return unplayed; }
            set { unplayed = value; }
        }

        public GameTreeEdge Incoming
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

        public GameTreeNode (Domain domain, Problem problem, int id) : this (domain, problem, null, new State(problem.Initial, null, null), id, 0) { }

        public GameTreeNode (Domain domain, Problem problem, State state, int id) : this (domain, problem, null, state, id, 0) { }

        //public GameTreeNode (Domain domain, Problem problem, GameTreeEdge incoming, int id) : this(domain, problem, incoming.Parent, incoming, incoming.Parent.State.NewState(incoming.Action as Operator, problem.Objects), id) { }

        public GameTreeNode (Domain domain, Problem problem, GameTreeEdge incoming, State state, int id, int depth)
        {
            this.domain = domain;
            this.problem = problem;
            this.incoming = incoming;
            this.id = id;
            this.state = state;
            this.depth = depth;

            outgoing = new List<GameTreeEdge>();
            unplayed = new List<GameTreeEdge>();

            timesPlayed = 0;
            wins = 0;

            if (state.Satisfies(problem.Goal)) satisfiesGoal = true;
            else satisfiesGoal = false;

            deadEnd = false;
        }

        public bool Play ()
        {
            if (satisfiesGoal)
            {
                AddResult(true);
                return true;
            }
            else if (deadEnd)
            {
                AddResult(false);
                return false;
            }

            // Create a new PDDL problem file based on this state.
            Writer.ProblemToPDDL(Parser.GetTopDirectory() + @"Benchmarks\" + domain.Name.ToLower() + @"\probrob.pddl", domain, problem, state.Predicates);
            Writer.DomainToPDDL(Parser.GetTopDirectory() + @"Benchmarks\" + domain.Name.ToLower() + @"\domrob.pddl", domain);

            // Create and populate a problem object based on the new file.
            Problem newProb = new Problem("rob", problem.OriginalName, problem.Domain, problem.Player, problem.Objects, state.Predicates, problem.Intentions, problem.Goal);
            Plan plan = Planners.FastDownward.Plan(domain, newProb);

            if (plan.Steps.Count > 0)
            {
                AddResult(true);
                return true;
            }

            deadEnd = true;
            AddResult(false);
            return false;
        }

        public void AddResult(bool won)
        {
            timesPlayed++;
            if (won) wins++;
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
