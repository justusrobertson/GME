using System.Collections.Generic;

using Mediation.PlanTools;
using Mediation.Utilities;
using Mediation.KnowledgeTools;
using Mediation.Interfaces;
using Mediation.Enums;
using Mediation.Planners;

namespace Mediation.MediationTree
{
    public static class EventRevisor
    {
        /// <summary>
        /// Given a planner, a mediation tree node, and a mediation tree, return an event revision plan and state to set for the node.
        /// </summary>
        /// <param name="planner">The planner to use.</param>
        /// <param name="node">The current node in the mediation tree.</param>
        /// <param name="tree">The mediation tree.</param>
        /// <returns>A plan state pair for the current node.</returns>
        public static void EventRevision (Planner planner, MediationTreeNode node, MediationTree tree)
        {
            // Compute the event revision problem and domain for the current node.
            Tuple<Domain, Problem> pair = GetEventRevisionPair(node, tree);

            // Use the planner to find an event revision plan.
            Plan plan = PlannerInterface.Plan(planner, pair.First, pair.Second);

            if (plan.Steps.Count > 0)
            {
                Tuple<Plan, State> planState = GetPlanStatePair(plan, node, tree);
                node.Plan = planState.First;
                node.State = planState.Second;
                node.Problem.Initial = node.State.Predicates;
            }
        }

        /// <summary>
        /// Given an event revision plan, a mediation tree node, and a mediation tree, returns the plan and state to set in the node.
        /// </summary>
        /// <param name="plan">The full event revision plan from the original initial state.</param>
        /// <param name="node">The current node in the mediation tree.</param>
        /// <param name="tree">The mediation tree object.</param>
        /// <returns>The current state to set in the node after event revision and the plan from the state forward.</returns>
        public static Tuple<Plan, State> GetPlanStatePair (Plan plan, MediationTreeNode node, MediationTree tree)
        {
            // Reformat the event revision plan to get a regular plan.
            Plan formattedPlan = ReformatPlan(plan, tree.Root.Domain, tree.Root.Problem);

            // Create a list to hold event revision steps.
            List<IOperator> pastSteps = new List<IOperator>();

            // Create another list to hold future plan steps.
            List<IOperator> futureSteps = new List<IOperator>();

            // Loop through the steps in the formatted event revision plan...
            for (int i = 0; i < formattedPlan.Steps.Count; i++)
                // If the step happened before the current state, it is an event revision step.
                if (i < node.Depth) pastSteps.Add(formattedPlan.Steps[i]);
                // If the step happens at or after the current state, it is a future plan step.
                else futureSteps.Add(formattedPlan.Steps[i]);

            // Create a new state object by cloning the initial state of the original planning problem.
            State current = tree.Root.State.Clone() as State;

            // Loop through each of the event revision steps...
            foreach (IOperator step in pastSteps)
                // And update the state object by applying the action.
                current = new State (current.ApplyAction(step as Operator, node.Problem.Objects));

            // Return the future plan along with the computed state.
            return new Tuple<Plan, State>(new Plan(node.Domain, node.Problem, futureSteps), current);
        }

        /// <summary>
        /// Given an event revision plan, along with its original domain and problem, returns an equivalent regular plan.
        /// </summary>
        /// <param name="plan">A plan with event revision steps.</param>
        /// <param name="domain">The original domain used to build the event revision.</param>
        /// <param name="problem">The original problem used to build the mediation tree.</param>
        /// <returns>A regular plan, equivalent to the output of event revision.</returns>
        public static Plan ReformatPlan (Plan plan, Domain domain, Problem problem)
        {
            // Create a list to hold the new plan steps.
            List<IOperator> newSteps = new List<IOperator>();

            // Loop through the steps in the event revision plan.
            foreach (IOperator step in plan.Steps)
            {
                // Split the title according to the * token placeholder.
                string[] title = step.Name.Split('*');

                // Find the operator template in the original domain that matches the event revision action.
                Operator template = new Operator("do-nothing");
                if (!title[0].Equals("do-nothing")) template = domain.Operators.Find(x => x.Name.Equals(title[0])).Template() as Operator;

                // If this is a proper event revision action...
                if (title.Length > 1)
                    // If this was an observed action...
                    if (title.Length - 1 == template.Arity)
                    {
                        // Loop through each of the template's terms...
                        for (int i = 0; i < template.Arity; i++)
                            // And bind the variable to the constant listed in the title.
                            template.AddBinding(template.TermAt(i), title[i + 1]);
                    }
                    // If this was an unobserved action...
                    else
                    {
                        // Bind the action's actor using the title.
                        template.AddBinding(template.TermAt(0), title[1]);

                        // Then loop through each of the template's terms...
                        for (int i = 1; i < template.Arity; i++)
                            // And bind the variable using the corresponding constant in the action.
                            template.AddBinding(template.TermAt(i), step.TermAt(i));
                    }
                // If this is not an event revision action, just clone it.
                else template = step.Clone() as Operator;

                // Add the translated action to the set of actions.
                newSteps.Add(template);
            }

            // Create a new plan object using the new set of actions and return it.
            return new Plan(domain, problem, newSteps);
        }

        /// <summary>
        /// Given a mediation tree and a node in the tree, returns a event revision domain and problem.
        /// </summary>
        /// <param name="node">The mediation tree node.</param>
        /// <param name="tree">The mediation tree.</param>
        /// <returns>The event revision problem and domain for the node.</returns>
        public static Tuple<Domain, Problem> GetEventRevisionPair (MediationTreeNode node, MediationTree tree)
        {
            // Create a clone of the node's original domain.
            Domain revisionDomain = node.Domain.Clone() as Domain;

            // Create a clone of the node's original problem.
            Problem revisionProblem = tree.Root.Problem.Clone() as Problem;

            // Get the series of actions and state literals the player has observed.
            List<Tuple<Operator, State>> playerKnowledge = GetWorldKnowledge(tree.Player, node, tree);

            // Rename the problem to the automated slot.
            revisionProblem.Name = "rob";

            // Add the initial state tracking literal to the problem's initial state.
            revisionProblem.Initial.Add(new Predicate("state-depth", new List<ITerm> { new Term("depth1", true) }, true));

            revisionDomain.AddTypePair("depth", "state");
            revisionDomain.Predicates.Add(new Predicate("state-depth", new List<ITerm> { new Term("?depth", "", "state") }, true));

            // For every operator in the cloned domain...
            foreach (Operator op in revisionDomain.Operators)
                // Add a state tracking precondition that prevents it from being applied until after the event revision operations have been performed.
                op.Preconditions.Add(new Predicate("state-depth", new List<ITerm> { new Term("depth" + playerKnowledge.Count.ToString(), true) }, true));

            // Loop through the action-state pairs in the player knowledge.
            for (int i = 1; i < playerKnowledge.Count; i++)
            {
                // For each of these, add a new state tracker to the list of objects.
                revisionProblem.Objects.Add(new Obj("depth" + i.ToString(), "depth"));

                // If the player did not observed the current action, add a set of unobserved templates to the list of operators.
                if (playerKnowledge[i].First == null) revisionDomain.Operators.AddRange(GetUnobservedActionTemplates(i, tree.GetTurnAtIndex(i - 1), node.Domain, playerKnowledge[i - 1].Second));
                // If the player did observe the current action, add the observed template to the list of operators.
                else revisionDomain.Operators.Add(GetObservedActionTemplate(i, playerKnowledge[i].First));
            }

            // Add the final state tracking object to the problem.
            revisionProblem.Objects.Add(new Obj("depth" + playerKnowledge.Count.ToString(), "depth"));

            // Return the domain-problem pair.
            return new Tuple<Domain, Problem> (revisionDomain, revisionProblem);
        }

        /// <summary>
        /// Given an unobserved action and a depth at which it occurs, creates a set of template operators that will preserve the character's observations during event revision.
        /// </summary>
        /// <param name="depth">The order in which the action occurs during the world history.</param>
        /// <param name="actor">The actor at this depth.</param>
        /// <param name="originalDomain">A copy of the original domain of the mediation tree.</param>
        /// <param name="playerKnowledge">A set of everything the character knows about the previous state.</param>
        /// <returns></returns>
        public static List<IOperator> GetUnobservedActionTemplates (int depth, string actor, Domain originalDomain, State playerKnowledge)
        {
            // Create a list to hold the new operator templates.
            List<IOperator> newTemplates = new List<IOperator>();

            // Loop through the operator templates in the original domain.
            foreach (Operator op in originalDomain.Operators)
            {
                // Clone the current template.
                Operator newOp = op.Clone() as Operator;

                // Modify the template's name to include the actor and depth.
                newOp.Name += "*" + actor + "*" + depth;

                // Store the terms of the original template.
                List<ITerm> terms = newOp.Terms;

                // Bind the template's actor to the input character.
                terms[0].Constant = actor;
                
                // Give the modified terms back to the template.
                newOp.Terms = terms;

                // Add a precondition that specifies the state depth of the operator.
                newOp.Preconditions.Add(new Predicate("state-depth", new List<ITerm> { new Term("depth" + depth.ToString(), true) }, true));

                // Add all the positive and negative things the player knows about the world.
                newOp.Preconditions.AddRange(playerKnowledge.Predicates);

                // Remove the current state tracker.
                newOp.Effects.Add(new Predicate("state-depth", new List<ITerm> { new Term("depth" + depth.ToString(), true) }, false));

                // Add an updated state tracker.
                newOp.Effects.Add(new Predicate("state-depth", new List<ITerm> { new Term("depth" + (depth + 1).ToString(), true) }, true));

                // Add the template to the list of templates.
                newTemplates.Add(newOp);
            }

            // Add the no op.
            Operator noOp = new Operator("do-nothing*" + actor + "*" + depth);

            // Add a precondition that specifies the state depth of the operator.
            noOp.Preconditions.Add(new Predicate("state-depth", new List<ITerm> { new Term("depth" + depth.ToString(), true) }, true));

            // Remove the current state tracker.
            noOp.Effects.Add(new Predicate("state-depth", new List<ITerm> { new Term("depth" + depth.ToString(), true) }, false));

            // Add an updated state tracker.
            noOp.Effects.Add(new Predicate("state-depth", new List<ITerm> { new Term("depth" + (depth + 1).ToString(), true) }, true));

            newTemplates.Add(noOp);

            // Return the list of templates.
            return newTemplates;
        }

        /// <summary>
        /// Given an observed action and a depth at which it occurs, creates a template operator that will preserve the action during event revision.
        /// </summary>
        /// <param name="depth">The order in which the action occurs during the world history.</param>
        /// <param name="observedAction">The observed action.</param>
        /// <returns>A template that preserves the action during event revision.</returns>
        public static Operator GetObservedActionTemplate (int depth, Operator observedAction)
        {
            // Create a placeholder for the new action's name.
            string newName = observedAction.Name;

            // Loop through each term in the observed action.
            foreach (Term term in observedAction.Terms)
                // Add the term to the action's name.
                newName += "*" + term.ToString();

            // Create a new operator using the name.
            Operator newOp = new Operator(newName);

            // Add the depth as the only precondition of the operator.
            newOp.Preconditions.Add(new Predicate("state-depth", new List<ITerm> { new Term("depth" + depth.ToString(), true) }, true));

            // Loop through the effects of the observed action.
            foreach (Predicate effect in observedAction.Effects)
                // And populate the new action with each.
                newOp.Effects.Add(effect.Clone() as Predicate);

            foreach (IAxiom conditional in observedAction.Conditionals)
                newOp.Conditionals.Add(conditional.Clone() as IAxiom);

            // Add an effect to remove the current depth counter.
            newOp.Effects.Add(new Predicate("state-depth", new List<ITerm> { new Term("depth" + depth.ToString(), true) }, false));

            // Add an effect to add the next depth counter.
            newOp.Effects.Add(new Predicate("state-depth", new List<ITerm> { new Term("depth" + (depth + 1).ToString(), true) }, true));

            // Return the operator.
            return newOp;
        }

        /// <summary>
        /// Given a mediation tree and a node in the tree, returns the node's world history.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="tree">The tree.</param>
        /// <returns>The path through the tree from the root to the node.</returns>
        public static List<MediationTreeNode> GetWorldHistory (MediationTreeNode node, MediationTree tree)
        {
            // Create an empty list object to hold the history.
            List<MediationTreeNode> history = new List<MediationTreeNode>();

            // Add the node to the world history.
            history.Add(node);

            // Remember the current node's ID.
            int current = node.ID;

            // While the current node's parent is not null...
            while (tree.GetParent(current) != -1)
            {
                // Store the current node's parent object.
                MediationTreeNode parent = tree.GetNode(tree.GetParent(current));

                // Remember the parent's ID.
                current = parent.ID;

                // Insert the parent node into the world history.
                history.Insert(0, parent);
            }

            // Return the world history.
            return history;
        }

        /// <summary>
        /// Given a character, node, and tree, returns the character's knowledge of the world history.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="node">The node that corresponds to the current state.</param>
        /// <param name="tree">The mediation tree.</param>
        /// <returns>A list of action-state pairs that represent what the character knows.</returns>
        public static List<Tuple<Operator, State>> GetWorldKnowledge (string character, MediationTreeNode node, MediationTree tree)
        {
            // An object to hold the character's knowledge of the world history.
            List<Tuple<Operator, State>> knowledge = new List<Tuple<Operator, State>>();

            // Get the objective world history.
            List<MediationTreeNode> history = GetWorldHistory(node, tree);

            // Loop through the nodes in the world history branch...
            foreach (MediationTreeNode current in history)
            {
                // Create a new action.
                Operator action = null;

                // Make sure the current node is not the root.
                if (current.Incoming != null)
                    // Check if the character observes the incoming action.
                    if (KnowledgeAnnotator.Observes(character, current.Incoming.Action, current.State.Predicates))
                        // If so, remember that the character observed the action.
                        action = current.Incoming.Action.Clone() as Operator;

                // Create a new state consisting only of terms the character observed.
                State state = new State (KnowledgeAnnotator.FullKnowledgeState(tree.Root.Domain.Predicates, tree.Root.Problem.ObjectsByType, current.State.Predicates, character));

                // Store the observed action-state pair as a tuple.
                knowledge.Add(new Tuple<Operator, State>(action, state));
            }

            // Return the sequence of observed action-state pairs.
            return knowledge;
        }
    }
}