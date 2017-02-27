using Mediation.Enums;
using Mediation.Interfaces;
using Mediation.Planners;
using Mediation.PlanTools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mediation.MediationTree
{
    public static class SuperpositionChooser
    {
        /// <summary>
        /// The delegate for a range of possible strategies to choose between differentiated sets of superposed states.
        /// </summary>
        /// <param name="observations">The sets of possible literals observed by the player mapped to the sets of states consistent with each observation.</param>
        /// <param name="node">The current node in the mediation tree.</param>
        /// <returns>A set of states chosen by the system.</returns>
        public delegate HashSet<State> Choose (Dictionary<List<IPredicate>, HashSet<State>> observations, MediationTreeNode node);

        /// <summary>
        /// Randomly chooses a set of superposed states.
        /// </summary>
        /// <param name="observations">The sets of possible literals observed by the player mapped to the sets of states consistent with each observation.</param>
        /// <param name="node">The current node in the mediation tree.</param>
        /// <returns>A set of states chosen by the system.</returns>
        public static HashSet<State> ChooseRandom (Dictionary<List<IPredicate>, HashSet<State>> observations, MediationTreeNode node)
        {
            // Create a new Random object.
            Random rand = new Random();
            
            // Choose a random number to map onto a key in the observation dictionary.
            int pickNum = rand.Next(0, observations.Keys.Count);

            // Grab the observation set associated with the chosen integer.
            List<IPredicate> pickPreds = observations.Keys.ToArray()[pickNum];

            // Return the set of states consistent with the observation.
            return observations[pickPreds];
        }

        /// <summary>
        /// Chooses a set of superposed states based on a simple utility function.
        /// </summary>
        /// <param name="observations">The sets of possible literals observed by the player mapped to the sets of states consistent with each observation.</param>
        /// <param name="node">The current node in the mediation tree.</param>
        /// <returns>A set of states chosen by the system.</returns>
        public static HashSet<State> ChooseUtility (Dictionary<List<IPredicate>, HashSet<State>> observations, MediationTreeNode node)
        {
            // Create a dictionary of utilities that map to sets of states.
            Dictionary<float, HashSet<State>> utilities = new Dictionary<float, HashSet<State>>();

            // If there is more than one observation the player can make.
            if (observations.Keys.Count > 1)
                // Loop through each set of observed literals.
                foreach (List<IPredicate> key in observations.Keys)
                {
                    // Store how many wins and losses the set of states has.
                    float wins = 0;
                    float losses = 0;

                    // Loop through each state in the set of states consistent with the current observation.
                    foreach (State state in observations[key])
                    {
                        // Store whether we are at a goal state.
                        bool satisfiesGoal = false;

                        // Store whether a goal state is reached by the planner.
                        bool win = true;

                        // Check whether the current state is a goal state.
                        if (state.Satisfies(node.Problem.Goal)) satisfiesGoal = true;

                        // Create a new problem object for the current state.
                        Problem problem = new Problem("rob", node.Problem.OriginalName, node.Problem.Domain, node.Problem.Player, node.Problem.Objects, state.Predicates, node.Problem.Intentions, node.Problem.Goal);

                        // Find a plan from the current state if one exists.
                        Plan plan = PlannerInterface.Plan(Planner.FastDownward, node.Domain, problem);

                        // Check if a goal has been reached by the state or the plan.
                        if (plan.Steps.Count == 0 && !satisfiesGoal) win = false;

                        // Record the win or loss.
                        if (win) wins++;
                        else losses++;
                    }

                    // Initialize the utility.
                    float utility = 0;

                    // If none of the states lost, go ahead and return the current state.
                    if (losses == 0) return observations[key];
                    // Otherwise, record the utility as wins divided by losses.
                    else utility = wins / losses;

                    // Map the utility onto the current state set in the dictionary.
                    utilities[utility] = observations[key];
                }
            else
            {
                // Store the single set of observations.
                List<IPredicate> key = observations.Keys.ToArray()[0];

                // Use the observations to return the single set of states.
                return observations[key];
            }

            // Return the set of states with the highest utility value.
            return utilities[utilities.Keys.Max()];
        }
    }
}
