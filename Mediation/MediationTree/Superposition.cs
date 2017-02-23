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

        public bool IsUndetermined(Predicate pred)
        {
            bool t = false;
            bool f = false;
            foreach (State state in States)
            {
                if (state.InState(pred)) t = true;
                else f = true;

                if (t && f) return true;
            }

            return false;
        }

        public bool IsTrue (Predicate pred)
        {
            foreach (State state in States)
                if (!state.InState(pred)) return false;

            return true;
        }

        public bool IsFalse (Predicate pred)
        {
            return !IsTrue(pred);
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
