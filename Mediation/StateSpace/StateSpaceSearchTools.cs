using System;

using Mediation.Enums;
using Mediation.PlanTools;
using Mediation.KnowledgeTools;

namespace Mediation.StateSpace
{
    public static class StateSpaceSearchTools
    {
        // Checks whether a node is a leaf or a branch.
        public static Boolean CheckLeaf (Plan plan, State state, StateSpaceNode node)
        {
            // Return any empty plans.
            if (plan.Steps.Count == 0)
            {
                // If the goal is satisfied...
                if (state.Satisfies(plan.GoalStep.Preconditions))
                {
                    // Differentiate a satisfied goal.
                    node.satisfiesGoal = true;

                    // Record that a goal state was reached.
                    StateSpaceMediator.GoalStateCount++;
                }
                else
                    // Record that a dead end state was reached.
                    StateSpaceMediator.DeadEndCount++;

                // Return that the node is a leaf.
                return true;
            }

            // Return that the node is a branch.
            return false;
        }

        // Creates and returns a new node.
        public static StateSpaceNode CreateNode (Planner planner, Domain domain, Problem problem, Plan plan, State state)
        {
            // Create a node for the current state.
            StateSpaceNode root = new StateSpaceNode();

            // Record that a node was created.
            StateSpaceMediator.NodeCount++;

            // Check if the node is a leaf.
            if (StateSpaceSearchTools.CheckLeaf(plan, state, root))
                return root;

            // Set the node's domain.
            root.domain = domain;

            // Set the node's problem.
            root.problem = problem;

            // Set the node's plan.
            root.plan = plan;

            // Set the node's state.
            root.state = state;

            // Find out what the player knows.
            root.problem.Initial = KnowledgeAnnotator.Annotate(problem.Initial, problem.Player);

            // Find all outgoing user actions from this state.
            root.outgoing = StateSpaceTools.GetAllPossibleActions(domain, problem, plan, state);

            // Record the number of outgoing edges.
            StateSpaceMediator.OutgoingEdgesCount = StateSpaceMediator.OutgoingEdgesCount + root.outgoing.Count;

            // Return the current node.
            return root;
        }
    }
}
