using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Mediation.Interfaces;
using Mediation.Enums;
using Mediation.PlanTools;


namespace Mediation.StateSpace
{
    public static class StateSpaceTools
    {
        private static Hashtable playerActions;

        // Checks to see if a user could possibly perform the action.
        private static bool ValidAction (Operator template)
        {
            if (template.Predicate.TermAt(0).Type.Equals("character"))
                return true;

            foreach (Predicate precon in template.Preconditions)
            {
                if (precon.Name.Equals("character") && precon.TermAtEquals(0, template.TermAt(0)))
                    return true;
            }

            return false;
        }

        // Computes all possible player actions.
        private static void ComputePlayerActions (Domain domain, Problem problem)
        {
            List<Operator> possibleActions = new List<Operator>();

            foreach (Operator template in domain.Operators)
            {
                if (ValidAction(template))
                {
                    List<Hashtable> bindings = new List<Hashtable>();
                    Hashtable binding = new Hashtable();
                    binding[template.TermAt(0)] = problem.Player.ToLower();
                    bindings.Add(binding);

                    for (int i = 1; i < template.Arity; i++)
                    {
                        List<Hashtable> lastBindings = new List<Hashtable>();
                        foreach (Hashtable lastBinding in bindings)
                            lastBindings.Add(lastBinding.Clone() as Hashtable);

                        List<Hashtable> newBindings = new List<Hashtable>();

                        foreach (IObject obj in problem.Objects)
                        {
                            string type = template.Predicate.TermAt(i).Type;
                            bool validBind = false;

                            if (type.Equals("") || obj.SubType.Equals(type))
                                validBind = true;
                            else
                                foreach (string objType in obj.Types)
                                    if (type.Equals(objType))
                                        validBind = true;

                            if (validBind)
                            {
                                List<Hashtable> theseBindings = new List<Hashtable>();
                                foreach (Hashtable bind in lastBindings)
                                {
                                    Hashtable thisBind = bind.Clone() as Hashtable;
                                    thisBind.Add(template.TermAt(i), obj.Name);
                                    theseBindings.Add(thisBind);
                                }
                                newBindings.AddRange(theseBindings);
                            }
                        }

                        bindings = newBindings;
                    }

                    foreach (Hashtable bind in bindings)
                    {
                        Operator boundAction = template.Clone() as Operator;
                        boundAction.Bindings = bind;
                        boundAction.Predicate.BindTerms(bind);
                        foreach (Predicate precondition in boundAction.Preconditions)
                            precondition.BindTerms(bind);
                        foreach (Predicate effect in boundAction.Effects)
                            effect.BindTerms(bind);
                        possibleActions.Add(boundAction);
                    }
                }
            }

            playerActions.Add(domain.Name + problem.OriginalName, possibleActions);
        }

        // Finds all possible player actions for a given domain/problem.
        public static List<Operator> GetAllPlayerActions (Domain domain, Problem problem)
        {
            if (playerActions == null)
                playerActions = new Hashtable();

            if (!playerActions.ContainsKey(domain.Name + problem.OriginalName))
                ComputePlayerActions(domain, problem);

            return playerActions[domain.Name + problem.OriginalName] as List<Operator>;
        }

        // Finds all possible player actions for the given state.
        public static List<Operator> GetPlayerActions (Domain domain, Problem problem, State state)
        {
            List<Operator> satisfiedActions = new List<Operator>();
            foreach (Operator action in GetAllPlayerActions(domain, problem))
                if (state.Satisfies(action.Preconditions))
                    satisfiedActions.Add(action.Clone() as Operator);

            return satisfiedActions;
        }

        // Given a state, return the spanning causal links.
        public static List<CausalLink> GetSpanningLinks (Plan plan)
        {
            // Create an empty list for the spanning causal links.
            List<CausalLink> spanningLinks = new List<CausalLink>();

            foreach (CausalLink link in plan.Dependencies)
                if (link.Span.Contains(plan.Steps.First()))
                    spanningLinks.Add(link);

            // Return the list of links.
            return spanningLinks;
        }

        // Create a list of applicable exceptional actions for a state.
        public static List<StateSpaceEdge> GetExceptionalActions (Domain domain, Problem problem, Plan plan, State state)
        {
            // Create a list of possible player actions in the current state.
            List<Operator> possibleActions = GetPlayerActions(domain, problem, state);

            // Create a list of the causal links that span the state.
            List<CausalLink> spanningLinks = (List<CausalLink>)GetSpanningLinks(plan);

            // Create an empty list of exceptional actions.
            List<StateSpaceEdge> exceptionalActions = new List<StateSpaceEdge>();

            // Loop through every possible action.
            foreach (Operator action in possibleActions)
            {
                // Loop through every action's effects.
                foreach (Predicate effect in action.Effects)
                    // Loop through every spanning link.
                    foreach (CausalLink link in spanningLinks)
                        // If the effect is the inverse of the protected predicate...
                        if (effect.IsInverse(link.Predicate))
                            // Add the current action to the list of exceptional actions.
                            exceptionalActions.Add(new StateSpaceEdge(action.Clone() as Operator, ActionType.Exceptional));
            }
                
            List<int> remove = new List<int>();

            // Remove duplicate actions.
            for (int i = 0; i < exceptionalActions.Count - 1; i++)
                for (int j = i + 1; j < exceptionalActions.Count; j++)
                    if (exceptionalActions[i].Action.ToString().Equals(exceptionalActions[j].Action.ToString()))
                        remove.Add(j);

            for (int i = exceptionalActions.Count; i >= 0; i--)
                if (remove.Contains(i))
                    exceptionalActions.RemoveAt(i);

            // Return the list of exceptional actions.
            return exceptionalActions;
        }

        // Create the applicable constituent action for a state.
        public static StateSpaceEdge GetConstituentAction (Domain domain, Problem problem, Plan plan, State state)
        {
            // Iterate through the plan to find the next player step.
            for (int i = 0; i < plan.Steps.Count; i++)
            {
                if (plan.Steps[i].Arity > 0)
                    // Check to see if the next step in the plan is performed by the player.
                    if (plan.Steps[i].TermAt(0).Equals(problem.Player))
                    {
                        // Create a new plan.
                        Plan newPlan = plan.Clone() as Plan;

                        // Remove the current step from the new plan.
                        newPlan.Steps.RemoveAt(i);

                        // Add the current step to the start of the steps.
                        newPlan.Steps.Insert(0, plan.Steps[i].Clone() as Operator);

                        // Check to see if the new plan is valid.
                        if (PlanSimulator.VerifyPlan(newPlan, state, problem.Objects))
                            // The current step is constituent.
                            return new StateSpaceEdge(plan.Steps[i].Clone() as Operator, ActionType.Constituent);

                        // Exit the loop.
                        i = plan.Steps.Count;
                    }
            }
            
            // Otherwise, return a blank step as the user's constituent action.
            return new StateSpaceEdge(new Operator("do nothing"), ActionType.Constituent);
        }

        // Get the set of exceptional and constituent edges.
        public static List<StateSpaceEdge> GetPossibleActions (Domain domain, Problem problem, Plan plan, State state)
        {
            // Make the set of outgoing edges.
            List<StateSpaceEdge> actions = new List<StateSpaceEdge>();

            // Find the current constituent action.
            StateSpaceEdge constituent = GetConstituentAction(domain, problem, plan, state);

            // Add the constituent action to the possible actions.
            actions.Add(constituent);

            // Find and add the current exceptional actions.
            List<StateSpaceEdge> exceptionals = GetExceptionalActions(domain, problem, plan, state);

            // Remove any exceptional actions that correspond to the constituent step.
            foreach (StateSpaceEdge exception in exceptionals)
                if (!exception.Action.Equals(constituent.Action))
                    actions.Add(exception);

            // Return the outgoing edges.
            return actions;
        }

        // Creates a list of all possible things the player can do.
        public static List<StateSpaceEdge> GetAllPossibleActions (Domain domain, Problem problem, Plan plan, State state)
        {
            // Create a list of possible player actions in the current state.
            List<Operator> possibleActions = GetPlayerActions(domain, problem, state);

            // Create a list of the causal links that span the state.
            List<CausalLink> spanningLinks = (List<CausalLink>)GetSpanningLinks(plan);

            // Create an empty list of exceptional actions.
            List<StateSpaceEdge> exceptionalActions = new List<StateSpaceEdge>();

            // Store the exceptional actions to be removed later.
            List<Operator> exceptions = new List<Operator>();

            // Loop through every possible action.
            foreach (Operator action in possibleActions)
            {
                // Loop through every action's effects.
                foreach (Predicate effect in action.Effects)
                    // Loop through every spanning link.
                    foreach (CausalLink link in spanningLinks)
                        // If the effect is the inverse of the protected predicate...
                        if (effect.IsInverse(link.Predicate))
                        {
                            // Add the current action to the list of exceptional actions.
                            exceptionalActions.Add(new StateSpaceEdge(action.Clone() as Operator, ActionType.Exceptional));

                            // Add the actual action to the exception list.
                            exceptions.Add(action);
                        }


                //Loop through the action's axioms
                foreach (IAxiom conditional in state.ApplicableConditionals(action, problem.Objects))
                    // Loop through every conditional's effects.
                    foreach (IPredicate effect in conditional.Effects)
                        // Loop through every spanning link.
                        foreach (IDependency link in spanningLinks)
                            // If the effect is the inverse of the protected predicate...
                            if (effect.IsInverse(link.Predicate))
                            {
                                // Add the current action to the list of exceptional actions.
                                exceptionalActions.Add(new StateSpaceEdge(action.Clone() as Operator, ActionType.Exceptional));

                                // Add the actual action to the exception list.
                                exceptions.Add(action);
                            }
            }
                
            // Remove the exceptions from the possible actions.
            foreach (Operator exception in exceptions)
                possibleActions.Remove(exception);

            // This is a hack to remove duplicate exceptional actions with a Remove By Property method.
            List<int> remove = new List<int>();

            // Remove duplicate actions.
            for (int i = 0; i < exceptionalActions.Count - 1; i++)
                for (int j = i + 1; j < exceptionalActions.Count; j++)
                    if (exceptionalActions[i].Action.Equals(exceptionalActions[j].Action))
                        remove.Add(j);

            for (int i = exceptionalActions.Count; i >= 0; i--)
                if (remove.Contains(i))
                    exceptionalActions.RemoveAt(i);

            // Create an empty list of consistent actions.
            List<StateSpaceEdge> consistentActions = new List<StateSpaceEdge>();

            // Loop through the remaining actions.
            foreach (Operator action in possibleActions)
                // Add the current action to the list of consistent actions.
                consistentActions.Add(new StateSpaceEdge(action.Clone() as Operator, ActionType.Consistent));

            // Make the set of outgoing edges.
            List<StateSpaceEdge> actions = new List<StateSpaceEdge>();

            // Find the current constituent action.
            StateSpaceEdge constituent = GetConstituentAction(domain, problem, plan, state);

            // Add the constituent action to the possible actions.
            actions.Add(constituent);

            // Remove any exceptional actions that correspond to the constituent step.
            foreach (StateSpaceEdge exception in exceptionalActions)
                if (!exception.Action.Equals(constituent.Action))
                    actions.Add(exception);

            // Remove any exceptional actions that correspond to the constituent step.
            foreach (StateSpaceEdge consistent in consistentActions)
                if (!consistent.Action.Equals(constituent.Action))
                    actions.Add(consistent);

            // Return the outgoing edges.
            return actions;
        }
    }
}
