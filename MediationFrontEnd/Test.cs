using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mediation.Interfaces;
using Mediation.FileIO;
using Mediation.PlanSpace;
using Mediation.PlanTools;
using Mediation.Planners;
using Mediation.StateSpace;
using Mediation.Enums;

using MediationFrontEnd.ConsoleGame;

namespace MediationFrontEnd
{
    class Test
    {
        static void Main(string[] args)
        {
            /*
            string domainName = "bank";
            Domain domain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + domainName + @"\domain.pddl", PlanType.StateSpace);
            Problem problem = Parser.GetProblem(Parser.GetTopDirectory() + @"Benchmarks\" + domainName + @"\prob01.pddl");
            Plan plan = FastDownward.Plan(domain, problem);
            Console.Out.WriteLine(plan);
            */

            MTGame.Play();

            //StateSpaceNode root = StateSpaceMediator.BuildTree(domain, problem, plan, plan.Initial as State, 2);
            //Writer.ToHTML(Parser.GetTopDirectory() + @"Output\", root);
        }
    }
}
