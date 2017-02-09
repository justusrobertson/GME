using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Mediation.Interfaces
{
    public interface IProblem
    {
        // Problems have a name.
        string Name { get; set; }

        // Problems have an original name.
        string OriginalName { get; set; }

        // Problems have a domain.
        string Domain { get; set; }

        // Problems have a player.
        string Player { get; set; }

        // Problems have a list of objects.
        List<IObject> Objects { get; set; }

        // Problems have an initial state.
        List<IPredicate> Initial { get; set; }

        // Characters may have intentions.
        List<IIntention> Intentions { get; set; }

        // Problems have a goal state.
        List<IPredicate> Goal { get; set; }

        // Problems can be cloned.
        Object Clone();
    }
}
