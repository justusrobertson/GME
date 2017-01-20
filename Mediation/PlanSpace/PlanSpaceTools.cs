using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Mediation.Interfaces;
using Mediation.PlanTools;


namespace Mediation.PlanSpace
{
    static class PlanSpaceTools
    {
        // Finds all possible player actions for the given state.
        public static List<Operator> GetPlayerActions (Domain domain, Problem problem, State state)
        {
            // Create a list of operators to hold the actions.
            List<Operator> playerActions = new List<Operator>();

            // Loop through the operators in the domain.
            foreach (Operator op in domain.Operators)
            {
                // Create a list to hold each operator's preconditions.
                List<IPredicate> preconditions = new List<IPredicate>();

                // Create clones of the preconditions.
                // Because OOP.
                foreach (IPredicate precon in op.Preconditions)
                    preconditions.Add((Predicate)precon.Clone());

                // Create a hashtable to hold a binding.
                Hashtable bind = new Hashtable();

                // Bind the first term to the player's name.
                bind.Add(op.Predicate.Terms.First(), problem.Player);

                // Find applicable actions performed by the player.
                state.AddApplicables(preconditions, bind);

                // Loop through each applicable binding.
                foreach (Hashtable binds in state.applicables)
                {
                    // Set the binding to the current operator.
                    //op.Bindings = binds;

                    // Clone the operator into our list of actions.
                    playerActions.Add((Operator)op.Clone());
                }

                // Clear the applicable actions.
                state.applicables = new List<Hashtable>();
            }

            // Return the list of actions.
            return playerActions;
        }

        // Given a state, return the spanning causal links.
        public static List<CausalLink> GetSpanningLinks (State state, Plan plan)
        {
            // Create an empty list for the spanning causal links.
            List<CausalLink> spanningLinks = new List<CausalLink>();

            // Store the position of the state's head and tail step.
            int stateTailIndex = plan.Steps.IndexOf(state.lastStep);
            int stateHeadIndex = plan.Steps.IndexOf(state.nextStep);

            // Loop through the plan's causal links.
            foreach (CausalLink link in plan.Dependencies)
            {
                // Store the position of the current link's head and tail step.
                int linkTailIndex = plan.Steps.IndexOf(link.Tail);
                int linkHeadIndex = plan.Steps.IndexOf(link.Head);

                // If the state's indexes are encompassed by the link's indexes
                // add the link to the list.
                if (stateTailIndex >= linkTailIndex)
                    if (stateHeadIndex <= linkHeadIndex)
                        spanningLinks.Add(link);
            }

            // Return the list of links.
            return spanningLinks;
        }

        // Create a list of applicable exceptional actions for a state.
        public static List<PlanSpaceEdge> GetExceptionalActions (Domain domain, Problem problem, Plan plan, State state)
        {
            // Create a list of possible player actions in the current state.
            List<Operator> possibleActions = GetPlayerActions(domain, problem, state);

            // Create a list of the causal links that span the state.
            List<CausalLink> spanningLinks = GetSpanningLinks(state, plan);

            // Create an empty list of exceptional actions.
            List<PlanSpaceEdge> exceptionalActions = new List<PlanSpaceEdge>();

            // Loop through every possible action.
            foreach (Operator action in possibleActions)
                // Loop through every action's effects.
                foreach (Predicate effect in action.Effects)
                    // Loop through every spanning link.
                    foreach (CausalLink link in spanningLinks)
                        // If the effect is the inverse of the protected predicate...
                        //if (effect.EqualToPred(link.GetBoundPredicate(), action.Bindings) && effect.Sign != link.Predicate.Sign)
                            // Add the current action to the list of exceptional actions.
                            exceptionalActions.Add(new PlanSpaceEdge((Operator)action.Clone(), (CausalLink)link.Clone(), (State)state.Clone()));

            // Return the list of exceptional actions.
            return exceptionalActions;
        }
    }
}
