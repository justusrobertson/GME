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
    public class Predicate : IPredicate
    {
        private string name;
        private List<ITerm> terms;
        private bool sign;
        private Hashtable observing;

        // Access the predicate's name.
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        // Access the predicate's terms.
        public List<ITerm> Terms
        {
            get { return terms; }
            set { terms = value; }
        }

        // Access the predicate's sign.
        public bool Sign
        {
            get { return sign; }
            set { sign = value; }
        }

        // Access the predicate's arity.
        public int Arity
        {
            get { return terms.Count; }
        }

        public Predicate ()
        {
            name = "";
            terms = new List<ITerm>();
            sign = true;
            observing = new Hashtable();
        }

        public Predicate (string name, List<ITerm> terms, bool sign)
        {
            this.name = name;
            this.terms = terms;
            this.sign = sign;
            this.observing = new Hashtable();
        }

        public Predicate(string name, List<ITerm> terms, bool sign, Hashtable observing)
        {
            this.name = name;
            this.terms = terms;
            this.sign = sign;
            this.observing = observing;
        }

        // Records an observation.
        public void Observes (string character, bool observation)
        {
            observing[character] = observation;
        }

        // Returns an observation.
        public bool Observing (string character)
        {
            if (observing.ContainsKey(character))
                return (bool)observing[character];
            else
                return false;
        }

        // Return the term at the nth position.
        public ITerm TermAt (int position)
        {
            // Check to see if the term exists.
            if (Arity >= position + 1)
                // Return the term.
                return terms.ElementAt(position);

            // Otherwise, fail gracefully.
            return new Term();
        }

        // Compares a term to a string.
        public bool TermAtEquals(int position, string compareAgainst)
        {
            if (TermAt(position).Bound)
                return TermAt(position).Constant.Equals(compareAgainst);
            else
                return TermAt(position).Variable.Equals(compareAgainst);
        }

        // Checks to see if another predicate is inverse.
        public bool IsInverse (IPredicate predicate)
        {
            Predicate pred = predicate as Predicate;
            if (ToStringPositive().Equals(pred.ToStringPositive()))
                if (Sign != pred.Sign)
                    return true;

            return false;
        }

        // Checks if two predicates are equal.
        public Boolean EqualToPred (Predicate pred, Hashtable binds)
        {
            // If the predicates share a name.
            if (pred.name.Equals(name))
            {
                // If the predicates have the same number of terms.
                if (pred.terms.Count == terms.Count)
                {
                    // Loop through the terms.
                    for (int i = 0; i < terms.Count; i++)
                    {
                        // If any two terms do not unify,
                        // fail.
                        if (!pred.terms[i].Equals(binds[terms[i]]))
                            return false;
                    }

                    // Otherwise, success!
                    return true;
                }
            }

            // Otherwise, fail.
            return false;
        }

        // Checks whether the predicate is in a state.
        public Boolean InState (List<IPredicate> state, Hashtable binds)
        {
            // Loop through the state's predicates.
            foreach (Predicate pred in state)
            {
                // If any of the predicates unify,
                // success!
                if (EqualToPred(pred, binds))
                    return true;
            }

            // Otherwise, fail.
            return false;
        }

        // Checks whether the predicate is in a state.
        public bool InState(List<IPredicate> state)
        {
            // Loop through the state's predicates.
            foreach (Predicate pred in state)
            {
                // If any of the predicates unify,
                // success!
                if (Equals(pred))
                {
                    if (sign)
                        return true;
                    else
                        return false;
                }
            }

            // Otherwise, fail.
            if (sign)
                return false;
            else
                return true;
        }

        // Checks for bad bindings.
        public Hashtable Extend (IPredicate match, Hashtable binds)
        {
            // Loop through the predicate's terms.
            for (int i = 0; i < terms.Count; i++)
            {
                // If the term is already bound...
                if (binds.ContainsKey(terms[i]))
                {
                    // If the bindings do not unify,
                    // fail.
                    if (!binds[terms[i]].Equals(match.Terms[i]))
                        return new Hashtable();
                }
                // If a binding was made in the domain file...
                else if (terms[i].Variable.ToCharArray().ElementAt(0) != '?')
                {
                    if (terms[i].Equals(match.Terms[i]))
                        binds[terms[i]] = terms[i];
                    else
                        return new Hashtable();
                }
                else
                {
                    // Otherwise, create a new binding.
                    binds[terms[i]] = match.Terms[i];
                } 
            }

            // Return the new bindings.
            return binds;
        }

        // Rewrites term variables to their bindings.
        public void BindTerms (Hashtable binds)
        {
            foreach (ITerm term in terms)
                if (binds.ContainsKey(term.Variable))
                    term.Constant = binds[term.Variable] as string;
        }

        public void BindTerm (string constant, int position)
        {
            TermAt(position).Constant = constant;
        }

        // Displays the contents of the predicate.
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (!sign)
                sb.Append("(not ");

            sb.Append("(" + name);

            foreach (ITerm term in terms)
                if (term.Bound)
                    sb.Append(" " + term.Constant);
                else
                    sb.Append(" " + term.Variable);

            sb.Append(")");

            if (!sign)
                sb.Append(")");

            return sb.ToString();
        }

        // Displays the contents of the predicate
        // without a (not ) predicate.
        public string ToStringPositive()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("(" + name);

            foreach (ITerm term in terms)
                if (term.Bound)
                    sb.Append(" " + term.Constant);
                else
                    sb.Append(" " + term.Variable);

            sb.Append(")");

            return sb.ToString();
        }

        // Displays the contents of the predicate
        // using input bindings.
        public string ToString(Hashtable binds)
        {
            StringBuilder sb = new StringBuilder();

            if (!sign)
                sb.Append("(not ");

            sb.Append("(" + name);

            foreach (ITerm term in terms)
                if (term.Bound)
                    sb.Append(" " + term.Constant);
                else
                    sb.Append(" " + term.Variable);

            sb.Append(")");

            if (!sign)
                sb.Append(")");

            return sb.ToString();
        }

        public IPredicate GetReversed()
        {
            IPredicate clone = Clone() as IPredicate;
            clone.Sign = !clone.Sign;
            return clone;
        }

        // Creates a clone of the predicate.
        public Object Clone()
        {
            string newName = name;

            List<ITerm> newTerms = new List<ITerm>();
            foreach (ITerm term in terms)
                newTerms.Add(term.Clone() as Term);

            bool newSign = sign;

            return new Predicate(newName, newTerms, newSign, (Hashtable)observing.Clone());
        }

        // Checks if two predicates are equal.
        public override bool Equals(Object obj)
        {
            // Store the object as a Predicate.
            Predicate predicate = obj as Predicate;

            // If the predicates share a name and sign.
            if (predicate.Name.Equals(Name) && predicate.Sign == Sign)
            {
                // If the predicates have the same number of terms.
                if (predicate.Arity == Arity)
                {
                    // Loop through the terms.
                    for (int i = 0; i < Arity; i++)
                    {
                        // If any two terms do not unify,
                        // fail.
                        if (!predicate.TermAt(i).Equals(TermAt(i)))
                            return false;
                    }

                    // Otherwise, success!
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
                hash = hash * 23 + Name.GetHashCode();

                foreach (ITerm term in Terms)
                    hash = hash * 23 + term.GetHashCode();

                return hash;
            }
        }
    }
}
