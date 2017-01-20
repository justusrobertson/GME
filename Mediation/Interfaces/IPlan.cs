using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Mediation.PlanTools;

namespace Mediation.Interfaces
{
    public interface IPlan
    {
        // Plans have a domain.
        Domain Domain { get; set; }

        // Plans have a problem.
        Problem Problem { get; set; }

        // Plans have an ordered list of steps.
        List<IOperator> Steps { get; set; }

        // There will be dependencies (or causal links) among the plan's steps.
        List<CausalLink> Dependencies { get; set; }

        // The plan will have an initial state.
        IState Initial { get; set; }

        // The plan will have a goal state.
        IState Goal { get; set; }

        // The plan can be cloned.
        Object Clone();
    }
}
