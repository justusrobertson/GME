using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Mediation.PlanTools;
using Mediation.Interfaces;
using Mediation.Planners;
using Mediation.FileIO;

namespace Mediation.PlanSpace
{
    public static class PlanSpaceMediator
    {
        // Returns the root of a mediation tree.
        public static PlanSpaceNode BuildTree (Domain domain, Problem problem, Plan plan, int depth)
        {
            // Create the root mediation node.
            PlanSpaceNode root = new PlanSpaceNode();

            // Assign the node's plan.
            root.plan = plan;

            // Assign the node's problem.
            root.problem = problem;

            // Make and populate a list of possible exceptional actions.
            root.outgoing = GetExceptionals(domain, problem, plan);

            // Return the node if the depth limit has been reached.
            if (depth == 0)
                return root;
            
            // Loop through the possible exceptional actions.
            foreach (PlanSpaceEdge action in root.outgoing)
            {
                // Create a new problem object for the exceptional action.
                Problem newProblem = NewProblem(domain, problem, action);

                // Find an accommodative solution for the exceptional action.
                Plan newPlan = FastDownward.Plan(domain, newProblem);

                // Create a new mediation node.
                PlanSpaceNode child = BuildTree(domain, newProblem, newPlan, depth - 1);

                // Assign the child's parent node.
                child.parent = root;

                // Assign the child's exceptional action.
                child.incoming = action;

                // Add the child to the root's children.
                root.children[action] = child;
            }

            // Return the root node.
            return root;
        }

        // Creates a new problem file and object.
        public static Problem NewProblem (Domain domain, Problem problem, PlanSpaceEdge flaw)
        {
            // Store the state that results from applying the exceptional action.
            List<IPredicate> initial = flaw.state.ApplyAction(flaw.action, problem.Objects);

            // Create a new PDDL problem file based on this state.
            Writer.ToPDDL(Parser.GetTopDirectory() + @"Benchmarks\" + domain.Name.ToLower() + @"\probrob.pddl", domain, problem, initial);

            // Create and populate a problem object based on the new file.
            return new Problem ("rob", problem.OriginalName, problem.Domain, problem.Player, problem.Objects, initial, problem.Goal);
        }

        // Returns a list of all exceptional actions in a plan.
        public static List<PlanSpaceEdge> GetExceptionals (Domain domain, Problem problem, Plan plan)
        {
            // Enumerate the plan's possible states.
            List<State> states = plan.CreateStateTree();

            // Create a list for possible exceptional actions.
            List<PlanSpaceEdge> exceptionals = new List<PlanSpaceEdge>();

            // For every possible state...
            foreach (State state in states)
                // Add all exceptional actions possible in the state to the list.
                exceptionals.AddRange(PlanSpaceTools.GetExceptionalActions(domain, problem, plan, state));

            // Return the list of possible exceptional actions.
            return exceptionals;
        }
    }
}
