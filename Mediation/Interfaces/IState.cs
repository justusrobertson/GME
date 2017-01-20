using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Mediation.PlanTools;

namespace Mediation.Interfaces
{
    public interface IState
    {
        // A state knows what is true in the world.
        Hashtable Table { get; set; }

        // A state knows what predicates are true.
        List<IPredicate> Predicates { get; set; }

        // A state is at a position in a plan.
        int Position { get; set; }

        // A state can tell if a predicate is currently true.
        bool InState(IPredicate predicate);

        // A state can tell if a list of predicates are satisfied.
        bool Satisfies(List<IPredicate> predicates);

        // A state can clone itself.
        Object Clone();
    }
}
