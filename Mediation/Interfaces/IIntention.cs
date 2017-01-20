using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mediation.Interfaces
{
    public interface IIntention
    {
        // Intentions are associated with characters.
        string Character { get; set; }

        // Intentions are specified with predicates.
        IPredicate Predicate { get; set; }

        // Intentions can be cloned.
        Object Clone();
    }
}
