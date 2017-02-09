using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Mediation.Interfaces;

namespace Mediation.PlanTools
{
    [Serializable]
    public class Axiom : IAxiom
    {
        private List<ITerm> terms;
        private List<IPredicate> preconditions;
        private List<IPredicate> effects;

        private Hashtable bindings;

        // Access the predicate's terms.
        public List<ITerm> Terms
        {
            get { return terms; }
            set { terms = value; }
        }

        // Access the axiom's preconditions.
        public List<IPredicate> Preconditions
        {
            get { return preconditions; }
            set { preconditions = value; }
        }

        // Access the axiom's effects.
        public List<IPredicate> Effects
        {
            get { return effects; }
            set { effects = value; }
        }

        // Access the axiom's bindings.
        public Hashtable Bindings
        {
            get { return bindings; }
            set { bindings = value; }
        }

        // Access the axiom's arity.
        public int Arity
        {
            get { return terms.Count; }
        }

        public Axiom ()
        {
            terms = new List<ITerm>();
            preconditions = new List<IPredicate>();
            effects = new List<IPredicate>();
            bindings = new Hashtable();
        }

        public Axiom (List<ITerm> terms, List<IPredicate> preconditions, List<IPredicate> effects, Hashtable bindings)
        {
            this.terms = terms;
            this.preconditions = preconditions;
            this.effects = effects;
            this.bindings = bindings;
        }

        // Lock the axiom's bindings.
        public void BindTerms ()
        {
            foreach (ITerm term in terms)
                term.Constant = bindings[term.Variable] as string;

            foreach (Predicate precondition in preconditions)
                precondition.BindTerms(bindings);

            foreach (Predicate effect in effects)
                effect.BindTerms(bindings);
        }

        // Return the term at the nth position.
        public string TermAt(int position)
        {
            // Check to see if the term exists.
            if (Arity >= position + 1)
                // Return the term.
                if (terms.ElementAt(position).Bound)
                    return terms.ElementAt(position).Constant;
                else
                    return terms.ElementAt(position).Variable;

            // Otherwise, fail gracefully.
            return "";
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Terms:");
            foreach (ITerm term in terms)
                if (term.Bound)
                    sb.Append(term.Constant + " ");
                else
                    sb.Append(term.Variable + " ");

            sb.AppendLine();
            sb.AppendLine("Preconditions:");
            foreach (Predicate precon in preconditions)
                sb.AppendLine(precon.ToString());

            sb.AppendLine();
            sb.AppendLine("Effects:");
            foreach (Predicate effect in effects)
                sb.AppendLine(effect.ToString());

            return sb.ToString();
        }

        // Checks if two operators are equal.
        public override bool Equals(Object obj)
        {
            // Store the object an axiom
            Axiom ax = obj as Axiom;

            // If the number of preconditions are the same...
            if (ax.Preconditions.Count == Preconditions.Count)
            {
                // Loop through the preconditions.
                foreach (Predicate precondition in Preconditions)
                    if (!ax.Preconditions.Contains(precondition))
                        return false;

                // If the number of effects are the same...
                if (ax.Effects.Count == Effects.Count)
                {
                    // Loop through the effects.
                    foreach (Predicate effect in Effects)
                        if (!ax.Effects.Contains(effect))
                            return false;

                    // Otherwise, return true!
                    return true;
                }
            }

            // Otherwise, fail.
            return false;
        }

        // Returns a hashcode.
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                // Suitable nullity checks etc, of course :)

                foreach (IPredicate pred in preconditions)
                    hash = hash * 23 + pred.GetHashCode();

                foreach (IPredicate pred in effects)
                    hash = hash * 23 + pred.GetHashCode();

                return hash;
            }
        }

        public Object Clone()
        {
            List<ITerm> newTerms = new List<ITerm>();
            foreach (ITerm term in Terms)
                newTerms.Add(term.Clone() as Term);

            List<IPredicate> newPreconditions = new List<IPredicate>();
            foreach (IPredicate prec in preconditions)
                newPreconditions.Add((Predicate)prec.Clone());

            List<IPredicate> newEffects = new List<IPredicate>();
            foreach (IPredicate effect in effects)
                newEffects.Add((Predicate)effect.Clone());

            Hashtable newBindings = new Hashtable();
            newBindings = bindings.Clone() as Hashtable;

            return new Axiom(newTerms, newPreconditions, newEffects, newBindings);
        }

        public Object Template()
        {
            Axiom clone = Clone() as Axiom;
            Hashtable newBinds = clone.Bindings;
            List<string> keys = new List<string>();
            foreach (string key in newBinds.Keys)
                keys.Add(key);
            foreach (string key in keys)
                newBinds[key] = "";
            clone.Bindings = newBinds;
            clone.BindTerms();
            return clone;
        }
    }
}
