using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mediation.FileIO;
using Mediation.PlanTools;
using Mediation.Enums;
using Mediation.Interfaces;
using Mediation.Planners;

namespace Mediation.Tests
{
    [TestClass]
    public class PlannerTest
    {
        string testDomainName;
        string testDomainDirectory;
        Domain testDomain;
        Problem testProblem;

        public PlannerTest ()
        {
            testDomainName = "zombie";
            testDomainDirectory = Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl";
            testDomain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl", PlanType.StateSpace);
            testProblem = Parser.GetProblem(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\prob01.pddl");
        }

        [TestMethod]
        public void FDMultiSinglePlan()
        {
            Plan testPlan = FDMulti.Plan(testDomain, testProblem);
            Assert.IsNotNull(testPlan);
        }

        [TestMethod]
        public void FDMultiPlan()
        {
            List<Plan> testPlans = FDMulti.MultiPlan(testDomain, testProblem);
            Assert.AreEqual(testPlans.Count, 10);
        }
    }
}
