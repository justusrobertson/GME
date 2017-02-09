using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Mediation.PlanTools;
using Mediation.Interfaces;

namespace Mediation.StateSpace
{
    public static class PlanSimulator
    {
        // Given a plan and the current state, verify it can be executed.
        public static bool VerifyPlan (Plan plan, State state, List<IObject> objects)
        {
            // If there are steps left in the plan...
            if (plan.Steps.Count > 0)
            {
                // If the next plan step can be executed...
                if (state.Satisfies(plan.Steps.First().Preconditions))
                {
                    // Create a new plan.
                    Plan newPlan = plan.Clone() as Plan;

                    // Remove the first step.
                    newPlan.Steps.RemoveAt(0);

                    // Update the world state and recursively call the function.
                    return VerifyPlan(newPlan, state.NewState(plan.Steps.First().Clone() as Operator, objects), objects);
                }
                else
                    // This is not a valid plan.
                    return false;
            }
            
            // This is a valid plan.
            return true;
        }
    }
}
