using Mediation.Enums;
using Mediation.Interfaces;
using Mediation.Planners;
using Mediation.PlanTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mediation.MediationTree
{
    public static class SuperpositionChooser
    {
        public delegate HashSet<State> Choose (Dictionary<List<IPredicate>, HashSet<State>> observations, MediationTreeNode node);

        public static HashSet<State> ChooseRandom (Dictionary<List<IPredicate>, HashSet<State>> observations, MediationTreeNode node)
        {
            Random rand = new Random();
            int pickNum = rand.Next(0, observations.Keys.Count);
            List<IPredicate> pickPreds = observations.Keys.ToArray()[pickNum];

            return observations[pickPreds];
        }

        public static HashSet<State> ChooseUtility (Dictionary<List<IPredicate>, HashSet<State>> observations, MediationTreeNode node)
        {
            Dictionary<float, HashSet<State>> utilities = new Dictionary<float, HashSet<State>>();

            if (observations.Keys.Count > 1)
                foreach (List<IPredicate> key in observations.Keys)
                {
                    float wins = 0;
                    float losses = 0;

                    foreach (State state in observations[key])
                    {
                        bool satisfiesGoal = false;
                        bool win = true;

                        if (state.Satisfies(node.Problem.Goal)) satisfiesGoal = true;

                        Problem problem = new Problem("rob", node.Problem.OriginalName, node.Problem.Domain, node.Problem.Player, node.Problem.Objects, state.Predicates, node.Problem.Intentions, node.Problem.Goal);
                        Plan plan = PlannerInterface.Plan(Planner.FastDownward, node.Domain, problem);

                        if (plan.Steps.Count == 0 && !satisfiesGoal) win = false;

                        if (win) wins++;
                        else losses++;
                    }

                    float utility = 0;

                    if (losses == 0) return observations[key];
                    else utility = wins / losses;

                    utilities[utility] = observations[key];
                }
            else
            {
                List<IPredicate> key = observations.Keys.ToArray()[0];
                return observations[key];
            }

            return utilities[utilities.Keys.Max()];
        }
    }
}
