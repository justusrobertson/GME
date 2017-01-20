using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mediation.PlanTools;
using Mediation.Planners;

namespace Mediation.Mediation
{ 
    class MediationEdge
    {
        public Operator action;
        public CausalLink clobberedLink;
        public State state;

        public MediationEdge ()
        {
            action = new Operator();
            clobberedLink = new CausalLink();
            state = new State();
        }

        public MediationEdge (Operator action, CausalLink clobberedLink, State state)
        {
            this.action = action;
            this.clobberedLink = clobberedLink;
            this.state = state;
        }

        // Displays the contents of the exceptional action.
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("ACTION");
            sb.AppendLine(action.ToStepString());
            sb.AppendLine("LINK");
            sb.AppendLine(clobberedLink.ToString());

            return sb.ToString();
        }
    }
}
