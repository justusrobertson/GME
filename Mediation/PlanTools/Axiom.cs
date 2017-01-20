using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Mediation.Interfaces;

namespace Mediation.PlanTools
{
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
    }
}
