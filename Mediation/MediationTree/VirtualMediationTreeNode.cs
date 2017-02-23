using Mediation.Enums;
using Mediation.Planners;
using Mediation.PlanTools;
using System;
using System.Collections.Generic;

namespace Mediation.MediationTree
{
    [Serializable]
    public class VirtualMediationTreeNode : MediationTreeNode
    {
        public VirtualMediationTreeNode(Domain domain, Problem problem, int id) : this (domain, problem, null, new Superposition(problem.Initial), null, id, 0) { }

        public VirtualMediationTreeNode(Domain domain, Problem problem, MediationTreeEdge incoming, State state, Plan plan, int id, int depth)
        {
            this.domain = domain;
            this.incoming = incoming;
            this.id = id;
            this.state = state.Clone() as State;
            this.depth = depth;
            this.plan = plan;

            // Create and populate a problem object based on the new file.
            this.problem = new Problem("rob", problem.OriginalName, problem.Domain, problem.Player, problem.Objects, this.state.Predicates, problem.Intentions, problem.Goal);

            outgoing = new List<MediationTreeEdge>();

            Superposition super = state as Superposition;
            satisfiesGoal = false;
            foreach (State st in super.States)
                if (st.Satisfies(problem.Goal))
                {
                    satisfiesGoal = true;
                    break;
                }

            UpdatePlan();
        }

        protected Problem GetProblem (State superState)
        {
            return new Problem("rob", problem.OriginalName, problem.Domain, problem.Player, problem.Objects, superState.Predicates, problem.Intentions, problem.Goal);
        }

        protected override void UpdatePlan()
        {
            if (incoming != null)
            {
                if (incoming.ActionType.Equals(ActionType.Exceptional))
                {
                    Superposition super = state as Superposition;
                    foreach (State current in super.States)
                    {
                        plan = PlannerInterface.Plan(Planner.FastDownward, domain, GetProblem(current));
                        if (plan.Steps.Count > 0) return;
                    }
                }
                else if (incoming.ActionType.Equals(ActionType.Constituent)) plan = plan.GetPlanUpdate(problem, incoming.Action as Operator);
            }
            else
            {
                Superposition super = state as Superposition;
                foreach (State current in super.States)
                {
                    plan = PlannerInterface.Plan(Planner.FastDownward, domain, GetProblem(current));
                    if (plan.Steps.Count > 0) return;
                }
            }
        }
    }
}
