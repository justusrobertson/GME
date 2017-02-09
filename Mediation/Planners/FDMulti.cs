﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

using Mediation.PlanTools;
using Mediation.FileIO;

namespace Mediation.Planners
{
    public static class FDMulti
    {
        // Creates and reads a plan into an object.
        public static Plan Plan (Domain domain, Problem problem)
        {
            // Create new PDDL problem and domain files.
            Writer.ProblemToPDDL(Parser.GetTopDirectory() + @"Benchmarks\" + domain.Name.ToLower() + @"\probrob.pddl", domain, problem, problem.Initial);
            Writer.DomainToPDDL(Parser.GetTopDirectory() + @"Benchmarks\" + domain.Name.ToLower() + @"\domrob.pddl", domain);

            // Start Fast Downward's batch file.
            ProcessStartInfo startInfo = new ProcessStartInfo(Parser.GetTopDirectory() + @"\fdmulti.bat");

            // Store the process' arguments.
            startInfo.Arguments = Parser.GetScriptDirectory() + " " + domain.Name + " " + problem.Name;

            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            // Start the process and wait for it to finish.
            using (Process proc = Process.Start(startInfo))
            {
                proc.WaitForExit();
            }

            // Erase old data.
            System.IO.File.WriteAllText(Parser.GetTopDirectory() + @"\FDMulti\output\output", string.Empty);
            System.IO.File.WriteAllText(Parser.GetTopDirectory() + @"\FDMulti\output\output.sas", string.Empty);

            // Parse the results into a plan object.
            return Parser.GetPlan(Parser.GetTopDirectory() + @"FDMulti\output\sas_plan.1", domain, problem);
        }

        // Creates and reads a plan into an object.
        public static List<Plan> MultiPlan (Domain domain, Problem problem)
        {
            // Create new PDDL problem and domain files.
            Writer.ProblemToPDDL(Parser.GetTopDirectory() + @"Benchmarks\" + domain.Name.ToLower() + @"\probrob.pddl", domain, problem, problem.Initial);
            Writer.DomainToPDDL(Parser.GetTopDirectory() + @"Benchmarks\" + domain.Name.ToLower() + @"\domrob.pddl", domain);

            // Start Fast Downward's batch file.
            ProcessStartInfo startInfo = new ProcessStartInfo(Parser.GetTopDirectory() + @"\fdmulti.bat");

            // Store the process' arguments.
            startInfo.Arguments = Parser.GetScriptDirectory() + " " + domain.Name + " " + problem.Name;

            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            // Start the process and wait for it to finish.
            using (Process proc = Process.Start(startInfo))
            {
                proc.WaitForExit();
            }

            // Erase old data.
            System.IO.File.WriteAllText(Parser.GetTopDirectory() + @"\FDMulti\output\output", string.Empty);
            System.IO.File.WriteAllText(Parser.GetTopDirectory() + @"\FDMulti\output\output.sas", string.Empty);

            List<Plan> plans = new List<Plan>();

            for (int i = 1; i < 11; i++) 
                plans.Add(Parser.GetPlan(Parser.GetTopDirectory() + @"FDMulti\output\sas_plan." + i, domain, problem));

            return plans;
        }
    }
}
