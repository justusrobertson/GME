using Mediation.Interfaces;
using Mediation.PlanTools;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Mediation.MediationTree
{
    [Serializable]
    public class Superposition : State
    {
        /// <summary>
        /// A set of perceptually equivalent states.
        /// </summary>
        public HashSet<State> States { get; set; }

        public Superposition()
        {
            States = new HashSet<State>();
        }

        public Superposition (List<IPredicate> predicates)
        {
            States = new HashSet<State>() { new State(predicates) };
        }

        public Superposition (Hashtable table, Operator lastStep, Operator nextStep, List<Hashtable> applicables, HashSet<State> states)
            : base (table, lastStep, nextStep, applicables)
        {
            States = states;
        }

        /// <summary>
        /// Returns whether a literal is superposed.
        /// </summary>
        /// <param name="pred">The state literal.</param>
        /// <returns>True or False depending on whether the literal is known to the player or not.</returns>
        public bool IsUndetermined(Predicate pred)
        {
            // Create a bool for whether the player knows the literal is true.
            bool t = false;

            // Create a bool for whether the player knows the literal is false.
            bool f = false;

            // Loop through each state in the set of states.
            foreach (State state in States)
            {
                // If the literal is in the state, store true.
                if (state.InState(pred)) t = true;
                // Otherwise store false.
                else f = true;

                // If we have found the literal stored as both true and false, return true.
                if (t && f) return true;
            }

            // Return false.
            return false;
        }

        /// <summary>
        /// Check whether a literal is true in the superposition.
        /// </summary>
        /// <param name="pred">The state literal.</param>
        /// <returns>True or False depending on whether the literal is known to be true.</returns>
        public bool IsTrue (Predicate pred)
        {
            // Loop through the states in the superposition.
            foreach (State state in States)
                // If the literal is false in the current state, return false.
                if (!state.InState(pred)) return false;

            // Return true.
            return true;
        }

        /// <summary>
        /// Check whether a literal is false in the superposition.
        /// </summary>
        /// <param name="pred">The state literal.</param>
        /// <returns>True or False depending on whether the literal is known to be false.</returns>
        public bool IsFalse (Predicate pred)
        {
            // Loop through the states in the superposition.
            foreach (State state in States)
                // If the literal is true in the current state, return false.
                if (state.InState(pred)) return false;

            // Return true.
            return true;
        }

        // Creates a clone of the state.
        public override Object Clone()
        {
            List<Hashtable> newApplicables = new List<Hashtable>();
            foreach (Hashtable applicable in applicables)
                newApplicables.Add((Hashtable)applicable.Clone());

            Operator newLastStep = new Operator();
            if (lastStep != null) newLastStep = lastStep.Clone() as Operator;

            Operator newNextStep = new Operator();
            if (nextStep != null) newNextStep = nextStep.Clone() as Operator;

            return new Superposition ((Hashtable)Table.Clone(), newLastStep, newNextStep, newApplicables, new HashSet<State>(States, States.Comparer));
        }
    }
}
