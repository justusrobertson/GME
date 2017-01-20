using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Mediation.PlanTools;

namespace Mediation.Interfaces
{
    public interface IDependency
    {
        // Dependencies protect a predicate.
        Predicate Predicate { get; set; }

        // Dependencies span from a tail step.
        IOperator Tail { get; set; }

        // Dependencies span to a head step.
        IOperator Head { get; set; }

        // Dependencies span a set of operators.
        List<IOperator> Span { get; set; }

        // Dependencies can be cloned.
        Object Clone();
    }
}
