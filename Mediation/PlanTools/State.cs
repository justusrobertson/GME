using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Mediation.Interfaces;
using Mediation.Utilities;

namespace Mediation.PlanTools
{
    [Serializable]
    public class State : IState
    {
        public Operator lastStep;
        public Operator nextStep;
        public List<Hashtable> applicables;

        private Hashtable table;
        private int position;

        // Access the current state model.
        public Hashtable Table
        {
            get { return table; }
            set { table = value; }
        }

        // Access the state's position.
        public int Position
        {
            get { return position; }
            set { position = value; }
        }

        // Access the current state predicates.
        public List<IPredicate> Predicates
        {
            get
            {
                // Create a new list of predicates.
                List<IPredicate> predicates = new List<IPredicate>();

                // Loop through the keys/predicates.
                foreach (IPredicate predicate in table.Keys)
                    // If the predicate is listed as true.
                    if ((bool)table[predicate])
                        // Store the predicate in the list.
                        predicates.Add(predicate);

                predicates.Sort(PredicateComparer.InverseCompareByName);

                // Return the list.
                return predicates;
            }

            set
            {
                // Loop through the predicates.
                foreach (IPredicate predicate in value)
                    if (!table.ContainsKey(predicate))
                        // And insert them in the new hashtable.
                        table.Add(predicate, true);
            }
        }

        public State ()
        {
            table = new Hashtable();
            lastStep = new Operator();
            nextStep = new Operator();
            applicables = new List<Hashtable>();
            position = -1;
        }

        public State(List<IPredicate> predicates) : this(predicates, null, null) { }

        public State(List<IPredicate> predicates, Operator lastStep, Operator nextStep)
        {
            table = new Hashtable();
            Predicates = predicates;
            this.lastStep = lastStep;
            this.nextStep = nextStep;
            this.applicables = new List<Hashtable>();
            position = -1;
        }

        public State(Hashtable table, Operator lastStep, Operator nextStep, List<Hashtable> applicables)
        {
            this.table = table;
            this.lastStep = lastStep;
            this.nextStep = nextStep;
            this.applicables = applicables;
            position = -1;
        }

        public State(List<IPredicate> predicates, Operator lastStep, Operator nextStep, List<Hashtable> applicables)
        {
            table = new Hashtable();
            Predicates = predicates;
            this.lastStep = lastStep;
            this.nextStep = nextStep;
            this.applicables = applicables;
            position = -1;
        }

        // Check to see if a predicate is true in the current state.
        public bool InState (IPredicate predicate)
        {
            if (predicate.Sign)
            {
                if (table.ContainsKey(predicate))
                    return (bool)table[predicate];
                else
                    return false;
            }
            else
            {
                Predicate reversed = predicate.Clone() as Predicate;
                reversed.Sign = true;
                if (table.ContainsKey(reversed))
                    return !(bool)table[reversed];
                else
                    return true;
            }
        }

        public bool SuperposedInState (IPredicate predicate)
        {
            if (predicate.Sign)
            {
                if (table.ContainsKey(predicate))
                    return (bool)table[predicate];
                else
                    return false;
            }
            else
            {
                Predicate reversed = predicate.Clone() as Predicate;
                reversed.Sign = true;
                if (table.ContainsKey(reversed))
                    return (bool)table[reversed];
                else
                    return false;
            }
        }

        // Checks to see if the state contains certain predicates.
        public bool Satisfies (List<IPredicate> predicates)
        {
            foreach (IPredicate predicate in predicates)
                if (!InState(predicate))
                    return false;

            return true;
        }

        /// <summary>
        /// Checks to see if a superposition contains certain predicates.
        /// </summary>
        /// <param name="predicates">The formulae that must be determined.</param>
        /// <param name="superposition">The maybe state.</param>
        /// <returns></returns>
        public bool Satisfies (List<IPredicate> predicates, State superposition)
        {
            foreach (IPredicate predicate in predicates)
                if (!InState(predicate) && !superposition.SuperposedInState(predicate))
                    return false;

            return true;
        }

        // Apply an operator's effects to the current state.
        public State NewState (Operator action, List<IObject> objects)
        {
            // Create a new state object.
            State state = new State();

            // Get the new state.
            state.Predicates = ApplyAction(action, objects);

            // Add the next and last steps.
            state.lastStep = action;
            //state.nextStep = action;

            // Return the new state.
            return state;
        }

        // Find applicable conditional effects.
        public List<IAxiom> ApplicableConditionals (Operator action, List<IObject> objects)
        {
            // Store the conditional axioms that are true in the current state.
            List<IAxiom> applicableEffects = new List<IAxiom>();

            // Loop through the action's conditional effect axioms.
            foreach (IAxiom conditional in action.Conditionals)
            {
                List<Hashtable> bindings = new List<Hashtable>();
                Hashtable binding = new Hashtable();

                if (conditional.Arity == 0)
                {
                    Hashtable thisBind = action.Bindings.Clone() as Hashtable;
                    bindings.Add(thisBind);
                }
                else
                {
                    for (int i = 0; i < conditional.Arity; i++)
                    {
                        List<Hashtable> lastBindings = new List<Hashtable>();
                        foreach (Hashtable lastBinding in bindings)
                            lastBindings.Add(lastBinding.Clone() as Hashtable);

                        List<Hashtable> newBindings = new List<Hashtable>();

                        foreach (IObject obj in objects)
                        {
                            List<Hashtable> theseBindings = new List<Hashtable>();
                            if (lastBindings.Count > 0)
                                foreach (Hashtable bind in lastBindings)
                                {
                                    Hashtable thisBind = bind.Clone() as Hashtable;
                                    thisBind.Add(conditional.TermAt(i), obj.Name);
                                    theseBindings.Add(thisBind);
                                }
                            else
                            {
                                Hashtable thisBind = action.Bindings.Clone() as Hashtable;
                                thisBind.Add(conditional.TermAt(i), obj.Name);
                                theseBindings.Add(thisBind);
                            }
                            newBindings.AddRange(theseBindings);
                        }

                        bindings = newBindings;
                    }
                }

                foreach (Hashtable bind in bindings)
                {
                    Axiom boundAxiom = conditional.Clone() as Axiom;
                    boundAxiom.Bindings = bind;
                    applicableEffects.Add(boundAxiom);
                }
            }

            List<IAxiom> removeCond = new List<IAxiom>();
            foreach (IAxiom conditional in applicableEffects)
            {
                conditional.BindTerms();
                if (!Satisfies(conditional.Preconditions))
                    removeCond.Add(conditional);
            }

            foreach (Axiom remove in removeCond)
                applicableEffects.Remove(remove);

            return applicableEffects;
        }

        // Apply an operator's effects to the current state.
        public List<IPredicate> ApplyAction (Operator action, List<IObject> objects)
        {
            // Create a new set of predicates.
            List<IPredicate> newPredicates = new List<IPredicate>();

            // Initialize the state to the previous state.
            foreach (IPredicate pred in Predicates)
                newPredicates.Add(pred.Clone() as Predicate);

            // Store the conditional axioms that are true in the current state.
            List<IAxiom> applicableEffects = ApplicableConditionals(action, objects);

            action.Conditionals = applicableEffects;

            List<IPredicate> effects = new List<IPredicate>();
            foreach (IPredicate effect in action.Effects)
                effects.Add(effect.Clone() as Predicate);

            foreach (IAxiom conditional in applicableEffects)
                foreach (IPredicate effect in conditional.Effects)
                    effects.Add(effect.Clone() as Predicate);

            // Update the state using the current step effects.
            foreach (IPredicate effect in effects)
            {
                // Bind the effect's terms for good.
                //effect.BindTerms(action.Bindings);

                // Add a positive effect that does not already exist.
                if (effect.Sign)
                    if (!newPredicates.Contains(effect))
                        newPredicates.Add(effect);

                // Hack because C# is silly... or maybe I am.
                Predicate found = new Predicate();

                // Find if a negative effect already exists.
                if (!effect.Sign)
                    foreach (IPredicate pred in newPredicates)
                        if (pred.ToString().Equals(effect.ToStringPositive()))
                            found = pred as Predicate;

                // Remove a negative effect.
                if (!found.Name.Equals(""))
                    newPredicates.Remove(found);
            }

            // Return the new set of predicates.
            return newPredicates;
        }

        // Given an operator's preconditions, finds applicable bindings for this state.
        public void AddApplicables(List<IPredicate> precs, Hashtable subs)
        {
            // Get the subset of positive preconditions.
            List<IPredicate> positivePrecs = precs.FindAll(x => x.Sign == true);

            // Check to see if any positive preconditions are left.
            if (positivePrecs.Count == 0)
            {
                // Get the subset of negative preconditions.
                List<IPredicate> negativePrecs = precs.FindAll(x => x.Sign == false);

                // For each negative precondition.
                foreach (IPredicate negPrec in negativePrecs)
                {
                    // If the negation exists in the world, fail.
                    if (negPrec.InState(Predicates, subs))
                        return;
                }

                // Otherwise, add the current bindings to applicables.
                applicables.Add(subs);
            }
            else
            {
                // Choose a positive precondition.
                Predicate positivePrec = positivePrecs.First() as Predicate;

                // For each predicate in the current state.
                foreach (IPredicate pred in Predicates)
                {
                    // If the precondition and predicate are the same type.
                    if (positivePrec.Name.Equals(pred.Name) && positivePrec.Arity == pred.Arity)
                    {
                        // Check if the two predicates can unify.
                        Hashtable newSubs = positivePrec.Extend(pred, (Hashtable)subs.Clone());

                        // If they unified...
                        if (newSubs.ContainsKey(positivePrec.Terms.First()) && pred.Sign)
                        {
                            // Clone the remaining preconditions, because pass by refs :(
                            List<IPredicate> newPrecs = new List<IPredicate>();
                            foreach (IPredicate precon in precs)
                                newPrecs.Add((Predicate)precon.Clone());

                            // Remove the current precondition from the cloned list.
                            newPrecs.Remove(newPrecs.Find(x => x.ToString().Equals(positivePrec.ToString())));

                            // Recursively invocate the method.
                            AddApplicables(newPrecs, newSubs);
                        }
                    }
                }
            }

            return;
        }

        public List<IPredicate> Difference (State other)
        {
            List<IPredicate> difference = new List<IPredicate>();
            foreach (Predicate pred in Predicates)
                if (!other.InState(pred)) difference.Add(pred);

            foreach (Predicate pred in other.Predicates)
                if (!InState(pred)) difference.Add(pred);

            return difference;
        }

        // Displays the contents of the state.
        public string ToStringDescriptive()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Last Step: " + lastStep);
            sb.AppendLine("Next Step: " + nextStep);

            sb.AppendLine("State Predicates");

            foreach (Predicate pred in Predicates)
                sb.AppendLine(pred.ToString());

            return sb.ToString();
        }

        // Displays the contents of the state.
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (Predicate pred in Predicates)
                sb.Append(pred.ToString());

            return sb.ToString();
        }

        // Checks if two predicates are equal.
        public override bool Equals(Object obj)
        {
            // Store the object as a State.
            State state = obj as State;

            if (Predicates.Count == state.Predicates.Count)
            {
                foreach (Predicate pred in Predicates)
                    if (!state.Predicates.Contains(pred))
                        return false;

                foreach (Predicate pred in state.Predicates)
                    if (!Predicates.Contains(pred))
                        return false;

                return true;
            }

            return false;
        }

        // Returns a hashcode.
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                // Suitable nullity checks etc, of course :)
                hash = hash * 23 + Table.GetHashCode();

                return hash;
            }
        }

        // Creates a clone of the state.
        public virtual Object Clone ()
        {
            List<Hashtable> newApplicables = new List<Hashtable>();
            foreach (Hashtable applicable in applicables)
                newApplicables.Add((Hashtable)applicable.Clone());

            Operator newLastStep = new Operator();
            if (lastStep != null) newLastStep = lastStep.Clone() as Operator;

            Operator newNextStep = new Operator();
            if (nextStep != null) newNextStep = nextStep.Clone() as Operator;

            return new State((Hashtable)table.Clone(), newLastStep, newNextStep, newApplicables);
        }
    }
}
