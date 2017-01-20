using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mediation.Enums;
using Mediation.Interfaces;
using Mediation.PlanTools;

using Mediation.FileIO;
using Mediation.Planners;
using Mediation.KnowledgeTools;

namespace Mediation.StateSpace
{
    public static class StateSpaceMediator
    {
        // Build the mediation tree.
        public static StateSpaceNode BuildTree (Planner planner, Domain domain, Problem problem, Plan plan, State state, int depth)
        {
            // Create a node for the current state.
            StateSpaceNode root = new StateSpaceNode();

            // Return any empty plans.
            if (plan.Steps.Count == 0)
            {
                // If the goal is satisfied...
                if (state.Satisfies(plan.GoalStep.Preconditions))
                    // Differentiate a satisfied goal.
                    root.problem = problem;

                // Return the leaf node.
                return root;
            }

            // Set the node's plan.
            root.plan = plan;

            // Set the node's problem.
            root.problem = problem;

            // Find out what the player knows.
            root.problem.Initial = KnowledgeAnnotator.Annotate(problem.Initial, problem.Player);

            // Set the node's state.
            root.state = state;

            // Find all outgoing user actions from this state.
            root.outgoing = StateSpaceTools.GetAllPossibleActions(domain, problem, plan, state);

            // Return the node if the depth limit has been reached.
            if (depth <= 0)
                return root;

            // Loop through the possible actions.
            foreach (StateSpaceEdge edge in root.outgoing)
            {
                StateSpaceNode child = ExpandTree(planner, domain, problem, plan.Clone() as Plan, state.Clone() as State, edge, depth);
                
                // Set the child's parent to this node.
                child.parent = root;

                // Add the child to the parent's collection of children, by way of the current action.
                root.children[edge] = child;
            }

            // Return the current node.
            return root;
        }

        // Expand an edge.
        public static StateSpaceNode ExpandTree (Planner planner, Domain domain, Problem problem, Plan plan, State state, StateSpaceEdge edge, int depth)
        {
            // Create a new problem object.
            Problem newProblem = new Problem();

            // Create a new plan object.
            Plan newPlan = new Plan();

            // Store actions the system takes.
            List<IOperator> systemActions = new List<IOperator>();

            // If the action is an exceptional step...
            if (edge.ActionType == ActionType.Exceptional)
            {
                // Create a new problem object.
                newProblem = NewProblem(domain, problem, state, edge.Action);

                // Find a new plan.
                if (planner.Equals(Planner.FastDownward))
                    newPlan = FastDownward.Plan(domain, newProblem);
                else if (planner.Equals(Planner.Glaive))
                    newPlan = Glaive.Plan(domain, newProblem);

                // If the action was accommodated and the first step of the new plan isn't taken by the player...
                if (newPlan.Steps.Count > 0)
                {
                    // Add the action to the system action list.
                    systemActions = GetSystemActions(newPlan, new List<string> { problem.Player }, new List<IOperator>());

                    // Update the problem object with the system's next move.
                    newProblem = NewProblem(domain, newProblem, new State(newProblem.Initial, newPlan.InitialStep, (Operator)newPlan.Steps.First()), systemActions);

                    // Update the plan with the system's next move.
                    newPlan = newPlan.GetPlanUpdate(newProblem, systemActions);
                }
            }
            // Otherwise, if the action is a consistent step...
            else if (edge.ActionType == ActionType.Consistent)
            {
                // Create a new problem object.
                newProblem = NewProblem(domain, problem, state, edge.Action);

                // Add the action to the system action list.
                systemActions = GetSystemActions(plan, new List<string> { problem.Player }, new List<IOperator>());

                // Create a new state.
                State newState = new State(newProblem.Initial, state.nextStep, (Operator)plan.Steps.First());

                // Create a new problem object.
                newProblem = NewProblem(domain, newProblem, newState, systemActions);

                // Create a new plan.
                newPlan = plan.GetPlanUpdate(newProblem, systemActions);
            }
            // Otherwise, the action is a constituent step...
            else
            {
                // If there are effects of the constituent action...
                if (edge.Action.Effects.Count > 0)
                {
                    // Create a new problem object.
                    newProblem = NewProblem(domain, problem, state, edge.Action);

                    // Create a new plan.
                    newPlan = plan.GetPlanUpdate(newProblem, edge.Action as Operator);
                }
                // Otherwise, initialize to the old problem and plan...
                else
                {
                    newProblem = problem;
                    newPlan = plan.Clone() as Plan;
                }

                // If there are still plan actions...
                if (newPlan.Steps.Count > 0)
                {
                    // Add the action to the system action list.
                    systemActions = GetSystemActions(newPlan, new List<string> { problem.Player }, new List<IOperator>());

                    // Update the problem object with the system's next move.
                    newProblem = NewProblem(domain, newProblem, new State(newProblem.Initial, newPlan.InitialStep, (Operator)newPlan.Steps.First()), systemActions);

                    // Update the plan with the system's next move.
                    newPlan = newPlan.GetPlanUpdate(newProblem, systemActions);
                }
            }

            // Create an empty child node.
            StateSpaceNode child = null;

            // If there are remaining plan steps...
            if (newPlan.Steps.Count > 0)
                // Build a new tree using the first step of the plan as the next step.
                child = BuildTree(planner, domain, newProblem, newPlan, new State(newProblem.Initial, newPlan.InitialStep, (Operator)newPlan.Steps.First()), depth - 1);
            else
                // Terminate the tree by adding a goal node.
                child = BuildTree(planner, domain, newProblem, newPlan, new State(newProblem.Initial, newPlan.InitialStep, newPlan.GoalStep), depth - 1);

            // Store the system actions.
            child.systemActions = systemActions;

            // Record the action that triggered the node's generation.
            child.incoming = edge;

            return child;
        }

        // Creates a list of actions for computer controlled characters to perform.
        public static List<IOperator> GetSystemActions (Plan plan, List<string> moved, List<IOperator> actions)
        {
            // Iterate through the plan to find the next player step.
            for (int i = 0; i < plan.Steps.Count; i++)
            {
                // Make sure the step has arity.
                if (plan.Steps[i].Arity > 0)
                {
                    // A placeholder to check if the character has already moved.
                    bool hasMoved = false;

                    // Loop through the characters that have moved.
                    foreach (string character in moved)
                        // If it matches the actor...
                        if (plan.Steps[i].TermAt(0).Equals(character))
                            // ...mark as already moved.
                            hasMoved = true;

                    // If the character has not moved yet.
                    if (!hasMoved)
                    {
                        // Create a new plan.
                        Plan newPlan = plan.Clone() as Plan;

                        // Remove the current step from the new plan.
                        newPlan.Steps.RemoveAt(i);

                        // Add the current step to the start of the steps.
                        newPlan.Steps.Insert(0, plan.Steps[i].Clone() as Operator);

                        // Check to see if the new plan is valid.
                        if (PlanSimulator.VerifyPlan(newPlan, plan.Initial as State, plan.Problem.Objects))
                        {
                            // Add the step to the list of actions.
                            actions.Add(newPlan.Steps[0]);

                            // Remove the first step from the plan.
                            newPlan.Steps.RemoveAt(0);

                            // Add the character to the list of moved characters.
                            moved.Add(plan.Steps[i].TermAt(0));

                            // Recursively call the function.
                            return GetSystemActions (newPlan, moved, actions);
                        }
                    }
                }
            }

            // Return the list of actions to take.
            return actions;
        }

        // Creates a new problem file and object.
        public static Problem NewProblem (Domain domain, Problem problem, State state, IOperator action)
        {
            // Store the state that results from applying the action.
            List<IPredicate> initial = state.ApplyAction(action as Operator, problem.Objects);

            // Create a new PDDL problem file based on this state.
            Writer.ToPDDL(Parser.GetTopDirectory() + @"Benchmarks\" + domain.Name.ToLower() + @"\probrob.pddl", domain, problem, initial);

            // Create and populate a problem object based on the new file.
            return new Problem("rob", problem.OriginalName, problem.Domain, problem.Player, problem.Objects, initial, problem.Intentions, problem.Goal);
        }

        // Creates a new problem file and object.
        public static Problem NewProblem(Domain domain, Problem problem, State state, List<IOperator> actions)
        {
            // Store the new states.
            State newState = state.Clone() as State;

            // Create the initial state.
            List<IPredicate> initial = new List<IPredicate>();

            // Make sure there are actions to take.
            if (actions.Count > 0)
            {
                // Iterate through the actions...
                for (int i = 0; i < actions.Count - 1; i++)
                    // ...and update the state with each action.
                    newState = newState.NewState(actions.ElementAt(i) as Operator, problem.Objects);

                // Apply the final action to find the predicates.
                initial = newState.ApplyAction(actions.Last() as Operator, problem.Objects);
            }
            else
            {
                // Otherwise, our new state is what happens after the player finishes their move.
                initial = newState.Predicates;
            }

            // Create a new PDDL problem file based on this state.
            Writer.ToPDDL(Parser.GetTopDirectory() + @"Benchmarks\" + domain.Name.ToLower() + @"\probrob.pddl", domain, problem, initial);

            // Create and populate a problem object based on the new file.
            return new Problem("rob", problem.OriginalName, problem.Domain, problem.Player, problem.Objects, initial, problem.Intentions, problem.Goal);
        }
    }
}