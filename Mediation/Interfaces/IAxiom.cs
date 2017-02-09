using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Mediation.PlanTools;

namespace Mediation.Interfaces
{
    public interface IAxiom
    {
        // Axioms have terms.
        List<ITerm> Terms { get; set; }

        // Axioms have preconditions.
        List<IPredicate> Preconditions { get; set; }

        // Axioms have effects.
        List<IPredicate> Effects { get; set; }

        // Axioms have bindings.
        Hashtable Bindings { get; set; }

        // Axioms have arity.
        int Arity { get; }

        // Lock the axiom's bindings.
        void BindTerms ();

        // Returns the term at a position.
        string TermAt (int position);

        // An axiom can clone itself.
        Object Clone();

        // An axiom knows its template form.
        Object Template();
    }
}
