using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mediation.Interfaces;
using Mediation.PlanTools;
using Mediation.FileIO;
using Mediation.Planners;
using Mediation.Enums;
using Mediation.StateSpace;

namespace MediationTest
{
    [TestClass]
    public class StateSpaceMediatorArthurTest
    {
        string testDomainName;
        string testDomainDirectory;
        Domain testDomain;
        Problem testProblem;
        Plan testPlan;

        public StateSpaceMediatorArthurTest ()
        {
            testDomainName = "arthur";
            testDomainDirectory = Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl";
            testDomain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl", PlanType.StateSpace);
            testProblem = Parser.GetProblemWithTypes(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\prob01.pddl", testDomain);
            testPlan = FastDownward.Plan(testDomain, testProblem);
        }

        [TestMethod]
        public void ArthurParserTest()
        {
            foreach (IPredicate pred in testProblem.Initial)
                if (pred.Name.Equals("connected") && pred.TermAt(0).Constant.Equals("clearing"))
                    Assert.AreEqual(pred.Terms.Count, 2);
        }

        [TestMethod]
        public void StateSpaceMediatorArthurActionTest()
        {
            StateSpaceNode root = StateSpaceMediator.BuildTree(Planner.FastDownward, testDomain, testProblem, testPlan, testPlan.Initial as State, 3);
            Assert.AreEqual(root.outgoing.Count, 3);
        }
    }
}
