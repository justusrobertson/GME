using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Mediation.Interfaces;
using Mediation.Enums;
using Mediation.PlanTools;
using Mediation.MediationTree;

namespace Mediation.StateSpace
{
    public static class StateSpaceTools
    {
        private static Hashtable actions;

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

        // Finds all possible player actions for the given state.
        public static List<Operator> GetPlayerActions (Domain domain, Problem problem, State state)
        {
            return GetActions(problem.Player.ToLower(), domain, problem, state);
        }

        // Computes all possible character actions.
        private static void ComputeCharacterActions (string character, Domain domain, Problem problem)
        {
            List<Operator> possibleActions = new List<Operator>();

            foreach (Operator template in domain.Operators)
            {
                if (ValidAction(template))
                {
                    List<Hashtable> bindings = new List<Hashtable>();
                    Hashtable binding = new Hashtable();
                    binding[template.TermAt(0)] = character;
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

            Hashtable problemTable = actions[domain.Name + problem.OriginalName] as Hashtable;

            problemTable.Add(character, possibleActions);
        }

        // Finds all possible actions for a given character, domain, and problem.
        public static List<Operator> GetAllActions (string character, Domain domain, Problem problem)
        {
            if (actions == null)
                actions = new Hashtable();

            string key = domain.Name + problem.OriginalName;

            if (!actions.ContainsKey(key))
                actions[key] = new Hashtable();

            Hashtable problemTable = actions[key] as Hashtable;

            if (!problemTable.ContainsKey(character))
                ComputeCharacterActions(character, domain, problem);

            return problemTable[character] as List<Operator>;
        }

        // Finds all possible actions for the given character and state.
        public static List<Operator> GetActions (string character, Domain domain, Problem problem, State state)
        {
            List<Operator> satisfiedActions = new List<Operator>();
            foreach (Operator action in GetAllActions(character, domain, problem))
                if (state.Satisfies(action.Preconditions))
                    satisfiedActions.Add(action.Clone() as Operator);

            return satisfiedActions;
        }

        // Finds all possible actions for the given character and state.
        public static List<Operator> GetActions (string character, Domain domain, Problem problem, Superposition superposition)
        {
            List<Operator> satisfiedActions = new List<Operator>();

            foreach (State state in superposition.States)
                satisfiedActions.AddRange(GetActions(character, domain, problem, state));

            return satisfiedActions.Distinct<Operator>().ToList<Operator>();
        }

        // Given a state, return the spanning causal links.
        public static List<CausalLink> GetSpanningLinks (Plan plan)
        {
            // Create an empty list for the spanning causal links.
            List<CausalLink> spanningLinks = new List<CausalLink>();

            if (plan.Steps.Count > 0)
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
                        {
                            // Clone the action.
                            Operator exceptional = action.Clone() as Operator;

                            // Identify the exceptional effect.
                            exceptional.ExceptionalEffects.Add(effect.Clone() as Predicate);

                            // Add the current action to the list of exceptional actions.
                            exceptionalActions.Add(new StateSpaceEdge(exceptional, ActionType.Exceptional));
                        }
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
            Operator wait = new Operator("do-nothing");
            wait.Terms.Add(new Term("actor", problem.Player, "character"));
            return new StateSpaceEdge(wait, ActionType.Constituent);
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

            // Store exceptional effects.
            Hashtable exceptionalEffects = new Hashtable();

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
                            // Identify the exceptional effect.
                            if (exceptionalEffects.ContainsKey(action.Predicate.ToString()))
                            {
                                List<IPredicate> list = exceptionalEffects[action.Predicate.ToString()] as List<IPredicate>;
                                list.Add(effect.Clone() as Predicate);
                                exceptionalEffects[action.Predicate.ToString()] = list;
                            }
                            else
                                exceptionalEffects[action.Predicate.ToString()] = new List<IPredicate> { effect.Clone() as Predicate };

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
                                // Identify the exceptional effect.
                                if (exceptionalEffects.ContainsKey(action.Predicate.ToString()))
                                {
                                    List<IPredicate> list = exceptionalEffects[action.Predicate.ToString()] as List<IPredicate>;
                                    list.Add(effect.Clone() as Predicate);
                                    exceptionalEffects[action.Predicate.ToString()] = list;
                                }
                                else
                                    exceptionalEffects[action.Predicate.ToString()] = new List<IPredicate> { effect.Clone() as Predicate };

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

            for (int i = exceptionalActions.Count - 1; i >= 0; i--)
                if (remove.Contains(i))
                    exceptionalActions.RemoveAt(i);
                else
                    exceptionalActions[i].Action.ExceptionalEffects = exceptionalEffects[exceptionalActions[i].Action.Predicate.ToString()] as List<IPredicate>;

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

        // Create the applicable constituent action for a state.
        public static MediationTreeEdge GetConstituentAction (MediationTreeNode node)
        {
            return GetConstituentAction (node.Problem.Player, node);
        }

        // Create the applicable constituent action for a state.
        public static MediationTreeEdge GetConstituentAction (string actor, MediationTreeNode node)
        {
            // Iterate through the plan to find the next player step.
            for (int i = 0; i < node.Plan.Steps.Count; i++)
            {
                if (node.Plan.Steps[i].Arity > 0)
                    // Check to see if the next step in the plan is performed by the player.
                    if (node.Plan.Steps[i].TermAt(0).Equals(actor))
                    {
                        // Create a new plan.
                        Plan newPlan = node.Plan.Clone() as Plan;

                        // Remove the current step from the new plan.
                        newPlan.Steps.RemoveAt(i);

                        // Add the current step to the start of the steps.
                        newPlan.Steps.Insert(0, node.Plan.Steps[i].Clone() as Operator);

                        // Check to see if the new plan is valid.
                        if (PlanSimulator.VerifyPlan(newPlan, node.State, node.Problem.Objects))
                            // The current step is constituent.
                            return new MediationTreeEdge(node.Plan.Steps[i].Clone() as Operator, ActionType.Constituent, node.ID);

                        // Exit the loop.
                        i = node.Plan.Steps.Count;
                    }
            }

            // Otherwise, return a blank step as the user's constituent action.
            Operator wait = new Operator("do-nothing");
            wait.Terms.Add(new Term("actor", actor, "character"));
            return new MediationTreeEdge (wait, ActionType.Constituent, node.ID);
        }

        // Creates a list of all possible things the player can do.
        public static List<MediationTreeEdge> GetAllPossibleActions (MediationTreeNode node)
        {
            return GetAllPossibleActions (node.Problem.Player.ToLower(), node);
        }

        // Creates a list of all possible things a character can do.
        public static List<MediationTreeEdge> GetAllPossibleActions(string character, MediationTreeNode node)
        {
            // Create a list of possible player actions in the current state.
            List<Operator> possibleActions = new List<Operator>();

            if (node is VirtualMediationTreeNode) possibleActions = GetActions(character, node.Domain, node.Problem, node.State as Superposition);
            else possibleActions = GetActions(character, node.Domain, node.Problem, node.State);


            // Create a list of the causal links that span the state.
            List<CausalLink> spanningLinks = (List<CausalLink>)GetSpanningLinks(node.Plan);

            // Create an empty list of exceptional actions.
            List<MediationTreeEdge> exceptionalActions = new List<MediationTreeEdge>();

            // Store the exceptional actions to be removed later.
            List<Operator> exceptions = new List<Operator>();

            // Store exceptional effects.
            Hashtable exceptionalEffects = new Hashtable();

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
                            // Identify the exceptional effect.
                            if (exceptionalEffects.ContainsKey(action.Predicate.ToString()))
                            {
                                List<IPredicate> list = exceptionalEffects[action.Predicate.ToString()] as List<IPredicate>;
                                list.Add(effect.Clone() as Predicate);
                                exceptionalEffects[action.Predicate.ToString()] = list;
                            }
                            else
                                exceptionalEffects[action.Predicate.ToString()] = new List<IPredicate> { effect.Clone() as Predicate };

                            // Add the current action to the list of exceptional actions.
                            exceptionalActions.Add(new MediationTreeEdge(action.Clone() as Operator, ActionType.Exceptional, node.ID));

                            // Add the actual action to the exception list.
                            exceptions.Add(action);
                        }


                //Loop through the action's axioms
                foreach (IAxiom conditional in node.State.ApplicableConditionals(action, node.Problem.Objects))
                    // Loop through every conditional's effects.
                    foreach (IPredicate effect in conditional.Effects)
                        // Loop through every spanning link.
                        foreach (IDependency link in spanningLinks)
                            // If the effect is the inverse of the protected predicate...
                            if (effect.IsInverse(link.Predicate))
                            {
                                // Identify the exceptional effect.
                                if (exceptionalEffects.ContainsKey(action.Predicate.ToString()))
                                {
                                    List<IPredicate> list = exceptionalEffects[action.Predicate.ToString()] as List<IPredicate>;
                                    list.Add(effect.Clone() as Predicate);
                                    exceptionalEffects[action.Predicate.ToString()] = list;
                                }
                                else
                                    exceptionalEffects[action.Predicate.ToString()] = new List<IPredicate> { effect.Clone() as Predicate };

                                // Add the current action to the list of exceptional actions.
                                exceptionalActions.Add(new MediationTreeEdge(action.Clone() as Operator, ActionType.Exceptional, node.ID));

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

            for (int i = exceptionalActions.Count - 1; i >= 0; i--)
                if (remove.Contains(i))
                    exceptionalActions.RemoveAt(i);
                else
                    exceptionalActions[i].Action.ExceptionalEffects = exceptionalEffects[exceptionalActions[i].Action.Predicate.ToString()] as List<IPredicate>;

            // Create an empty list of consistent actions.
            List<MediationTreeEdge> consistentActions = new List<MediationTreeEdge>();

            // Loop through the remaining actions.
            foreach (Operator action in possibleActions)
                // Add the current action to the list of consistent actions.
                consistentActions.Add(new MediationTreeEdge(action.Clone() as Operator, ActionType.Consistent, node.ID));

            // Make the set of outgoing edges.
            List<MediationTreeEdge> actions = new List<MediationTreeEdge>();

            // Find the current constituent action.
            MediationTreeEdge constituent = GetConstituentAction(character, node);

            // Add the constituent action to the possible actions.
            actions.Add(constituent);

            // Remove any exceptional actions that correspond to the constituent step.
            foreach (MediationTreeEdge exception in exceptionalActions)
                if (!exception.Action.Equals(constituent.Action))
                    actions.Add(exception);

            // Remove any exceptional actions that correspond to the constituent step.
            foreach (MediationTreeEdge consistent in consistentActions)
                if (!consistent.Action.Equals(constituent.Action))
                    actions.Add(consistent);

            // Return the outgoing edges.
            return actions;
        }
    }
}
