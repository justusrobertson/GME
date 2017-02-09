using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Mediation.PlanTools
{
    [Serializable]
    public class Flaw
    {
        public Predicate precondition;
        public Operator step;

        public Flaw ()
        {
            precondition = new Predicate();
            step = new Operator();
        }

        public Flaw (Predicate precondition, Operator step)
        {
            this.precondition = precondition;
            this.step = step;
        }

        // Displays the contents of the flaw.
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Flaw: " + precondition);
                
            sb.AppendLine("Step: " + step);

            return sb.ToString();
        }
    }
}
