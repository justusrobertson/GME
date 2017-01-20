using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mediation.PlanTools;
using Mediation.FileIO;
using Mediation.Planners;
using Mediation.Enums;
using Mediation.StateSpace;

namespace MediationTest
{
    [TestClass]
    public class StateSpaceMediatorZombieTest
    {
        string testDomainName;
        string testDomainDirectory;
        Domain testDomain;
        Problem testProblem;
        Plan testPlan;

        public StateSpaceMediatorZombieTest ()
        {
            testDomainName = "zombie";
            testDomainDirectory = Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl";
            testDomain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl", PlanType.StateSpace);
            testProblem = Parser.GetProblem(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\prob01.pddl");
            testPlan = FastDownward.Plan(testDomain, testProblem);
        }

        [TestMethod]
        public void StateSpaceMediatorZombieConstituentUpdateTest()
        {
            StateSpaceNode root = StateSpaceMediator.BuildTree(Planner.FastDownward, testDomain, testProblem, testPlan, testPlan.Initial as State, 1);
            StateSpaceNode child = root.children[root.outgoing[0]] as StateSpaceNode;
            Assert.AreNotEqual(root.problem.Initial, child.problem.Initial);
        }
    }
}
