using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

using Mediation.PlanTools;
using Mediation.FileIO;

namespace Mediation.Planners
{
    public static class Glaive
    {
        // Creates and reads a plan into an object.
        public static Plan Plan(Domain domain, Problem problem)
        {
            // Start Fast Downward's batch file.
            ProcessStartInfo startInfo = new ProcessStartInfo(Parser.GetTopDirectory() + @"\glaive.bat");

            // Store the process' arguments.
            startInfo.Arguments = Parser.GetTopDirectory() + " " + domain.Name + " " + problem.Name;

            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            // Start the process and wait for it to finish.
            using (Process proc = Process.Start(startInfo))
            {
                proc.WaitForExit();
            }

            // Parse the results into a plan object.
            return Parser.GetPlan(Parser.GetTopDirectory() + @"Glaive\output\output", domain, problem);
        }
    }
}
