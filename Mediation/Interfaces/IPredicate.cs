using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Mediation.Interfaces
{
    public interface IPredicate
    {
        // Predicates have a name.
        string Name { get; set; }

        // Predicates have terms.
        List<ITerm> Terms { get; set; }

        // Predicates have a sign.
        bool Sign { get; set; }

        // Predicates have an arity.
        int Arity { get; }

        // Records an observation.
        void Observes (string character, bool observation);

        // Returns an observation.
        bool Observing (string character);

        // Returns the term at the nth position.
        ITerm TermAt(int position);

        // Compares a term to a string.
        bool TermAtEquals(int position, string compareAgainst);

        // Checks to see if another predicate is inverse.
        bool IsInverse(IPredicate predicate);

        // Checks to see if the predicate is in a state.
        bool InState(List<IPredicate> state, Hashtable binds);

        // Rewrites term variables to their bindings.
        void BindTerms (Hashtable binds);

        // Displays the content of the predicate without a not.
        string ToStringPositive();

        IPredicate GetReversed();

        // Predicates can be cloned.
        Object Clone();
    }
}
