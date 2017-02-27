using Mediation.Interfaces;
using Mediation.KnowledgeTools;
using Mediation.PlanTools;
using Mediation.Utilities;
using System.Collections.Generic;

namespace Mediation.MediationTree
{
    public static class SuperpositionManipulator
    {
        /// <summary>
        /// Collapses the superposition given the player, the current tree node, and a strategy for choosing between unique state sets 
        /// </summary>
        /// <param name="player">The player's name.</param>
        /// <param name="node">The current tree node.</param>
        /// <param name="chooser">A strategy for choosing between perceptually unique state sets.</param>
        /// <returns>A state superposition.</returns>
        public static HashSet<State> Collapse(string player, VirtualMediationTreeNode node, SuperpositionChooser.Choose chooser)
        {
            // Store the node's state as a superposition structure.
            Superposition super = node.State as Superposition;

            // Check to see if the node has more than one superposed state.
            if (super.States.Count > 0)
            {
                // Create a dictionary that maps sets of observed literals to sets of states that match the observation.
                Dictionary<List<IPredicate>, HashSet<State>> obs = new Dictionary<List<IPredicate>, HashSet<State>>(new PredicateListComparer());

                // Iterate through each state in the superposition.
                foreach (State state in super.States)
                {
                    // Find and store the set of literals observed by the player in the current state.
                    List<IPredicate> observed = KnowledgeAnnotator.FullKnowledgeState(node.Domain.Predicates, node.Problem.ObjectsByType, state.Predicates, player);

                    // If the current set of literals has been encountered before, add the state to its set.
                    if (obs.ContainsKey(observed)) obs[observed].Add(state);
                    // Otherwise, create a new dictionary key for the set of observed literals and initialize the state set with the current state.
                    else obs.Add(observed, new HashSet<State> { state });
                }

                // Choose a set of states from the superposition according to the given strategy and return it.
                return chooser(obs, node);
            }

            // Return the empty set of states.
            return super.States;
        }
    }
}
