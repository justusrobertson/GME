using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

using Mediation.PlanTools;

using Mediation.FileIO;

namespace Mediation.Planners
{
    public static class FastDownward
    {
        // Creates and reads a plan into an object.
        public static Plan Plan (Domain domain, Problem problem, string plan)
        {
            // Start Fast Downward's batch file.
            ProcessStartInfo startInfo = new ProcessStartInfo(Parser.GetTopDirectory() + @"\plan.bat");

            // Store the process' arguments.
            startInfo.Arguments = Parser.GetScriptDirectory() + " " + domain.Name + " " + problem.name;

            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            // Start the process and wait for it to finish.
            using (Process proc = Process.Start(startInfo))
            {
                proc.WaitForExit();
            }

            // Erase old data.
            System.IO.File.WriteAllText(Parser.GetTopDirectory() + @"\FastDownward\output\output", string.Empty);
            System.IO.File.WriteAllText(Parser.GetTopDirectory() + @"\FastDownward\output\output.sas", string.Empty);

            // Parse the results into a plan object.
            return Parser.GetPlan(plan, domain, problem);
        }
    }
}
