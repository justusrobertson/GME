using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Mediation.PlanTools;

namespace Mediation.Interfaces
{
    public interface IOperator
    {
        // Actions have a predicate.
        IPredicate Predicate { get; set; }

        // Action predicates have a name.
        string Name { get; set; }

        // Action predicates have terms.
        List<ITerm> Terms { get; set; }

        // Action predicates have arity.
        int Arity { get; }

        // Actions have preconditions.
        List<IPredicate> Preconditions { get; set; }

        // Actions have effects.
        List<IPredicate> Effects { get; set; }

        // Actions have conditional effects.
        List<IAxiom> Conditionals { get; set; }

        // Exceptional actions have exceptional effects.
        List<IPredicate> ExceptionalEffects { get; set; }

        // Actions have an actor.
        string Actor { get; }

        // Actions may have a list of consenting agents.
        List<ITerm> ConsentingAgents { get; set; }

        // Actions have a unique ID.
        int ID { get; }

        // Returns the term at the nth position.
        string TermAt(int position);

        // Actions can be cloned.
        Object Clone();

        Object Template();
    }
}
