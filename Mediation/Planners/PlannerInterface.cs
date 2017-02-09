using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mediation.Enums;
using Mediation.PlanTools;

namespace Mediation.Planners
{
    public class PlannerInterface
    {
        // Creates and reads a plan into an object.
        public static Plan Plan (Planner planner, Domain domain, Problem problem)
        {
            if (planner.Equals(Planner.FastDownward)) return FastDownward.Plan(domain, problem);
            else if (planner.Equals(Planner.FDMulti)) return FDMulti.Plan(domain, problem);
            else if (planner.Equals(Planner.Glaive)) return Glaive.Plan(domain, problem);

            return null;
        }
    }
}
