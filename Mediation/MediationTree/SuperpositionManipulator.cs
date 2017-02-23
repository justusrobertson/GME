using Mediation.Interfaces;
using Mediation.KnowledgeTools;
using Mediation.PlanTools;
using Mediation.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mediation.MediationTree
{
    public static class SuperpositionManipulator
    {
        /// <summary>
        /// Given the player and the current tree node, collapses the superposition based on the player's current knowledge.
        /// </summary>
        /// <param name="player">The player's name.</param>
        /// <param name="node">The current tree node.</param>
        /// <returns></returns>
        public static HashSet<State> Collapse (string player, VirtualMediationTreeNode node, SuperpositionChooser.Choose chooser)
        {
            Superposition super = node.State as Superposition;
            if (super.States.Count > 0)
            {
                Dictionary<List<IPredicate>, HashSet<State>> obs = new Dictionary<List<IPredicate>, HashSet<State>>(new PredicateListComparer());
                foreach (State state in super.States)
                {
                    List<IPredicate> observed = KnowledgeAnnotator.FullKnowledgeState(node.Domain.Predicates, node.Problem.ObjectsByType, state.Predicates, player);
                    if (obs.ContainsKey(observed)) obs[observed].Add(state);
                    else obs.Add(observed, new HashSet<State> { state });
                }

                return chooser(obs, node);
            }

            return super.States;
        }
    }
}
