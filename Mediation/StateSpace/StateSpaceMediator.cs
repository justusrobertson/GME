using System.Collections.Generic;
using System.Linq;

using Mediation.Enums;
using Mediation.Interfaces;
using Mediation.PlanTools;

using Mediation.FileIO;
using Mediation.Planners;
using Mediation.KnowledgeTools;
using Mediation.Utilities;

namespace Mediation.StateSpace
{
    public static class StateSpaceMediator
    {
        public static bool CEDeletion = true;

        public static int GoalStateCount = 0;

        public static int DeadEndCount = 0;

        public static int NodeCount = 0;

        public static int Constituent = 0;

        public static int Consistent = 0;

        public static int Exceptional = 0;

        public static int OutgoingEdgesCount = 0;

        public static void Clear ()
        {
            GoalStateCount = 0;
            DeadEndCount = 0;
            NodeCount = 0;
            Constituent = 0;
            Consistent = 0;
            Exceptional = 0;
            OutgoingEdgesCount = 0;
            StateSpaceNode.Counter = -1;
        }

        // Given a tree layer, creates the next layer of child nodes and returns them.
        public static List<StateSpaceNode> BuildLayer (Planner planner, List<StateSpaceNode> currentLayer)
        {
            // Create a new list for the child nodes.
            List<StateSpaceNode> childLayer = new List<StateSpaceNode>();

            // Loop through the nodes in the current layer.
            foreach (StateSpaceNode node in currentLayer)
                // Loop through all outgoing edges of the current node.
                foreach (StateSpaceEdge edge in node.outgoing)
                {
                    // Generate the child node.
                    StateSpaceNode child = CreateChild(planner, node.domain, node.problem, node.plan, node.state, edge);

                    // Set the child's parent to this node.
                    child.parent = node;

                    // Add the child to the parent's collection of children, by way of the current action.
                    node.children[edge] = child;

                    // Add the node to the child layer
                    childLayer.Add(child);
                }

            // Return the child layer.
            return childLayer;
        }

        // Creates a child node.
        private static StateSpaceNode CreateChild (Planner planner, Domain domain, Problem problem, Plan plan, State state, StateSpaceEdge edge)
        {
            // Create a new problem object.
            Problem newProblem = new Problem();

            // Create a new domain object.
            Domain newDomain = domain;

            // Create a new plan object.
            Plan newPlan = new Plan();

            // Store actions the system takes.
            List<IOperator> systemActions = new List<IOperator>();

            // If the outgoing action is an exceptional step...
            if (edge.ActionType == ActionType.Exceptional)
            {
                // Count the exceptional edge.
                Exceptional++;

                // Create a new problem object.
                newProblem = NewProblem(newDomain, problem, state, edge.Action);

                // Find a new plan.
                if (planner.Equals(Planner.FastDownward))
                    newPlan = FastDownward.Plan(newDomain, newProblem);
                else if (planner.Equals(Planner.Glaive))
                    newPlan = Glaive.Plan(newDomain, newProblem);

                // If the action was accommodated and the first step of the new plan isn't taken by the player...
                if (newPlan.Steps.Count > 0)
                {
                    // Add the action to the system action list.
                    systemActions = GetSystemActions(newPlan, new List<string> { problem.Player }, new List<IOperator>());

                    // Update the problem object with the system's next move.
                    newProblem = NewProblem(newDomain, newProblem, new State(newProblem.Initial, newPlan.InitialStep, (Operator)newPlan.Steps.First()), systemActions);

                    // Update the plan with the system's next move.
                    newPlan = newPlan.GetPlanUpdate(newProblem, systemActions);
                }
                else if (CEDeletion)
                {
                    // Try to find a domain revision alibi.
                    Tuple<Domain, Operator> alibi = ReviseDomain(planner, newDomain, problem, state, edge.Action as Operator);

                    // If domain revision worked.
                    if (alibi != null)
                    {
                        // Remember the new domain.
                        newDomain = alibi.First;

                        // Push the modified action to the edge object.
                        edge.Action = alibi.Second;

                        // Create a new problem object.
                        newProblem = NewProblem(newDomain, problem, state, edge.Action);

                        // Find a new plan.
                        if (planner.Equals(Planner.FastDownward))
                            newPlan = FastDownward.Plan(newDomain, newProblem);
                        else if (planner.Equals(Planner.Glaive))
                            newPlan = Glaive.Plan(newDomain, newProblem);

                        // Add the action to the system action list.
                        systemActions = GetSystemActions(newPlan, new List<string> { problem.Player }, new List<IOperator>());

                        // Update the problem object with the system's next move.
                        newProblem = NewProblem(newDomain, newProblem, new State(newProblem.Initial, newPlan.InitialStep, (Operator)newPlan.Steps.First()), systemActions);

                        // Update the plan with the system's next move.
                        newPlan = newPlan.GetPlanUpdate(newProblem, systemActions);
                    }
                }
            }
            // Otherwise, if the action is a consistent step...
            else if (edge.ActionType == ActionType.Consistent)
            {
                // Count the consistent edge.
                Consistent++;

                // Create a new problem object.
                newProblem = NewProblem(newDomain, problem, state, edge.Action);

                // Add the action to the system action list.
                systemActions = GetSystemActions(plan, new List<string> { problem.Player }, new List<IOperator>());

                // Create a new state.
                State newState = new State(newProblem.Initial, state.nextStep, (Operator)plan.Steps.First());

                // Create a new problem object.
                newProblem = NewProblem(newDomain, newProblem, newState, systemActions);

                // Create a new plan.
                newPlan = plan.GetPlanUpdate(newProblem, systemActions);
            }
            // Otherwise, the action is a constituent step...
            else
            {
                // Count the constituent edge.
                Constituent++;

                // If there are effects of the constituent action...
                if (edge.Action.Effects.Count > 0)
                {
                    // Create a new problem object.
                    newProblem = NewProblem(newDomain, problem, state, edge.Action);

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
                    newProblem = NewProblem(newDomain, newProblem, new State(newProblem.Initial, newPlan.InitialStep, (Operator)newPlan.Steps.First()), systemActions);

                    // Update the plan with the system's next move.
                    newPlan = newPlan.GetPlanUpdate(newProblem, systemActions);
                }
            }

            // Add the system actions to the current edge.
            edge.SystemActions = systemActions;

            // Create an empty child node.
            StateSpaceNode child = null;

            // If there are remaining plan steps...
            if (newPlan.Steps.Count > 0)
                // Build a new tree using the first step of the plan as the next step.
                child = StateSpaceSearchTools.CreateNode(planner, newDomain, newProblem, newPlan, new State(newProblem.Initial, newPlan.InitialStep, (Operator)newPlan.Steps.First()));
            else
                // Terminate the tree by adding a goal node.
                child = StateSpaceSearchTools.CreateNode(planner, newDomain, newProblem, newPlan, new State(newProblem.Initial, newPlan.InitialStep, newPlan.GoalStep));

            // Store the system actions.
            child.systemActions = systemActions;

            // Record the action that triggered the node's generation.
            child.incoming = edge;

            return child;
        }

        // Revises a domain to remove any possible exceptional conditional effect.
        public static Tuple<Domain, Operator> ReviseDomain (Planner planner, Domain domain, Problem problem, State state, Operator incoming)
        {
            // Create a new plan object.
            Plan newPlan = new Plan();

            // Examine each exceptional effect.
            foreach (Predicate effect in incoming.ExceptionalEffects)
            {
                // Create a new domain object.
                Domain newDomain = new Domain();

                // Save the domain's name.
                newDomain.Name = domain.Name;
                newDomain.staticStart = domain.staticStart;

                // If the current effect is conditional...
                if (incoming.IsConditional(effect))
                {
                    // If the conditional effect has not been observed by the player.

                    // Remove the conditional effect from the domain.

                    // Copy the current operator templates into a list.
                    List<IOperator> templates = new List<IOperator>();
                    foreach (IOperator templ in domain.Operators)
                        templates.Add(templ.Clone() as IOperator);

                    // Create a clone of the incoming action's unbound operator template.
                    IOperator temp = incoming.Template() as Operator;

                    // Find the incoming action template in the domain list.
                    Operator template = templates.Find(t => t.Equals(temp)) as Operator;

                    // Remove the incoming action template from the domain list.
                    templates.Remove(template);

                    // Create a clone of the incoming action.
                    Operator clone = incoming.Clone() as Operator;

                    // Create a list of conditional effects to remove from the template.
                    List<IAxiom> remove = new List<IAxiom>();
                    foreach (IAxiom conditional in clone.Conditionals)
                        if (conditional.Effects.Contains(effect))
                            remove.Add(conditional);

                    // Remove each conditional effect from the template.
                    foreach (IAxiom rem in remove)
                        template.Conditionals.Remove(rem.Template() as IAxiom);

                    // Add the modified template to the domain list.
                    templates.Add(template);

                    // Push the modified list to the new domain object.
                    newDomain.Operators = templates;

                    // Write new problem and domain files.
                    Writer.ProblemToPDDL(Parser.GetTopDirectory() + @"Benchmarks\" + domain.Name.ToLower() + @"\probrob.pddl", newDomain, problem, state.Predicates);
                    Writer.DomainToPDDL(Parser.GetTopDirectory() + @"Benchmarks\" + domain.Name.ToLower() + @"\domrob.pddl", newDomain);

                    // Find a new plan.
                    if (planner.Equals(Planner.FastDownward))
                        newPlan = FastDownward.Plan(newDomain, problem);
                    else if (planner.Equals(Planner.Glaive))
                        newPlan = Glaive.Plan(newDomain, problem);

                    // If the modified domain can accommodate the player's action...
                    if (newPlan.Steps.Count > 0)
                    {
                        // Clone the modified incoming action template.
                        Operator newAction = template.Clone() as Operator;

                        // Bind the cloned action with the incoming action's bindings.
                        newAction.Bindings = incoming.Bindings;

                        // Expand the tree using the new domain.
                        return new Tuple<Domain, Operator> (newDomain, newAction);
                    }
                }
            }

            return null;
        }

        // Build the mediation tree using a depth-first strategy.
        public static StateSpaceNode BuildTree (Planner planner, Domain domain, Problem problem, Plan plan, State state, int depth)
        {
            // Create a node for the current state.
            StateSpaceNode root = new StateSpaceNode();

            // Record that a node was created.
            NodeCount++;

            // Return any empty plans.
            if (plan.Steps.Count == 0)
            {
                // If the goal is satisfied...
                if (state.Satisfies(plan.GoalStep.Preconditions))
                {
                    // Differentiate a satisfied goal.
                    root.satisfiesGoal = true;

                    // Record that a goal state was reached.
                    GoalStateCount++;
                }
                else
                    // Record that a dead end state was reached.
                    DeadEndCount++;

                // Return the leaf node.
                return root;
            }

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
                // Count the exceptional edge.
                Exceptional++;

                // Create a new problem object.
                newProblem = NewProblem(domain, problem, state, edge.Action);

                // Find a new plan.
                newPlan = PlannerInterface.Plan(planner, domain, newProblem);

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
                else if (CEDeletion)
                {
                    StateSpaceNode alibi = GenerateAlibi(planner, domain, problem, plan, state, edge, depth);
                    if (alibi != null)
                        return alibi;
                }
            }
            // Otherwise, if the action is a consistent step...
            else if (edge.ActionType == ActionType.Consistent)
            {
                // Count the consistent edge.
                Consistent++;

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
                // Count the constituent edge.
                Constituent++;

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

            // Add the system actions to the current edge.
            edge.SystemActions = systemActions;

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
            Writer.ProblemToPDDL(Parser.GetTopDirectory() + @"Benchmarks\" + domain.Name.ToLower() + @"\probrob.pddl", domain, problem, initial);
            Writer.DomainToPDDL(Parser.GetTopDirectory() + @"Benchmarks\" + domain.Name.ToLower() + @"\domrob.pddl", domain);

            // Create and populate a problem object based on the new file.
            return new Problem("rob", problem.OriginalName, problem.Domain, problem.Player, problem.Objects, initial, problem.Intentions, problem.Goal);
        }

        // Creates a new problem file and object.
        public static Problem NewProblem (Domain domain, Problem problem, State state, List<IOperator> actions)
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
            Writer.ProblemToPDDL(Parser.GetTopDirectory() + @"Benchmarks\" + domain.Name.ToLower() + @"\probrob.pddl", domain, problem, initial);
            Writer.DomainToPDDL(Parser.GetTopDirectory() + @"Benchmarks\" + domain.Name.ToLower() + @"\domrob.pddl", domain);

            // Create and populate a problem object based on the new file.
            return new Problem("rob", problem.OriginalName, problem.Domain, problem.Player, problem.Objects, initial, problem.Intentions, problem.Goal);
        }
    
        // Rewrites history using alibi generation.
        public static StateSpaceNode GenerateAlibi (Planner planner, Domain domain, Problem problem, Plan plan, State state, StateSpaceEdge edge, int depth)
        {
            return RemoveConditionalEffects(planner, domain, problem, plan, state, edge, depth);
        }

        // Rewrites history to remove harmful conditional effects.
        public static StateSpaceNode RemoveConditionalEffects (Planner planner, Domain domain, Problem problem, Plan plan, State state, StateSpaceEdge edge, int depth)
        {
            // Store the incoming action.
            Operator incoming = edge.Action as Operator;

            // Create a new plan object.
            Plan newPlan = new Plan();

            // Examine each exceptional effect.
            foreach (Predicate effect in incoming.ExceptionalEffects)
            {
                // Create a new domain object.
                Domain newDomain = new Domain();

                // Save the domain's name.
                newDomain.Name = domain.Name;
                newDomain.staticStart = domain.staticStart;

                // If the current effect is conditional...
                if (incoming.IsConditional(effect))
                {
                    // If the conditional effect has not been observed by the player.

                    // Remove the conditional effect from the domain.

                    // Copy the current operator templates into a list.
                    List<IOperator> templates = new List<IOperator>();
                    foreach (IOperator templ in domain.Operators)
                        templates.Add(templ.Clone() as IOperator);

                    // Create a clone of the incoming action's unbound operator template.
                    IOperator temp = incoming.Template() as Operator;

                    // Find the incoming action template in the domain list.
                    Operator template = templates.Find(t => t.Equals(temp)) as Operator;

                    // Remove the incoming action template from the domain list.
                    templates.Remove(template);

                    // Create a clone of the incoming action.
                    Operator clone = incoming.Clone() as Operator;

                    // Create a list of conditional effects to remove from the template.
                    List<IAxiom> remove = new List<IAxiom>();
                    foreach (IAxiom conditional in clone.Conditionals)
                        if (conditional.Effects.Contains(effect))
                            remove.Add(conditional);

                    // Remove each conditional effect from the template.
                    foreach (IAxiom rem in remove)
                        template.Conditionals.Remove(rem.Template() as IAxiom);

                    // Add the modified template to the domain list.
                    templates.Add(template);

                    // Push the modified list to the new domain object.
                    newDomain.Operators = templates;

                    // Write new problem and domain files.
                    Writer.ProblemToPDDL(Parser.GetTopDirectory() + @"Benchmarks\" + domain.Name.ToLower() + @"\probrob.pddl", newDomain, problem, state.Predicates);
                    Writer.DomainToPDDL(Parser.GetTopDirectory() + @"Benchmarks\" + domain.Name.ToLower() + @"\domrob.pddl", newDomain);

                    // Find a new plan.
                    if (planner.Equals(Planner.FastDownward))
                        newPlan = FastDownward.Plan(newDomain, problem);
                    else if (planner.Equals(Planner.Glaive))
                        newPlan = Glaive.Plan(newDomain, problem);

                    // If the modified domain can accommodate the player's action...
                    if (newPlan.Steps.Count > 0)
                    {
                        // Clone the modified incoming action template.
                        Operator newAction = template.Clone() as Operator;

                        // Bind the cloned action with the incoming action's bindings.
                        newAction.Bindings = incoming.Bindings;

                        // Push the modified action to the edge object.
                        edge.Action = newAction;

                        // Expand the tree using the new domain.
                        return ExpandTree(planner, newDomain, problem, plan, state, edge, depth);
                    }
                }
            }

            return null;
        }
    }
}