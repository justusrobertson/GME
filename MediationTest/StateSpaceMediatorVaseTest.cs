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
    public class StateSpaceMediatorVaseTest
    {
        string testDomainName;
        string testDomainDirectory;
        Domain testDomain;
        Problem testProblem;
        Plan testPlan;

        public StateSpaceMediatorVaseTest ()
        {
            testDomainName = "vase";
            testDomainDirectory = Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl";
            testDomain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl", PlanType.StateSpace);
            testProblem = Parser.GetProblem(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\prob01.pddl");
            testPlan = FastDownward.Plan(testDomain, testProblem);
        }

        [TestMethod]
        public void StateSpaceMediatorVaseComputerUpdateTest()
        {
            StateSpaceNode root = StateSpaceMediator.BuildTree(Planner.FastDownward, testDomain, testProblem, testPlan, testPlan.Initial as State, 3);
            StateSpaceNode child = root.children[root.outgoing[0]] as StateSpaceNode;
            Assert.IsTrue(child.plan.Initial.Predicates.Contains(new Predicate("red", new List<ITerm> { new Term("vase", true) }, true)));
        }

        [TestMethod]
        public void StateSpaceMediatorVaseComputerUpdateTest2()
        {
            StateSpaceNode root = StateSpaceMediator.BuildTree(Planner.FastDownward, testDomain, testProblem, testPlan, testPlan.Initial as State, 3);
            StateSpaceNode child = root.children[root.outgoing[1]] as StateSpaceNode;
            Assert.IsTrue(child.plan.Initial.Predicates.Contains(new Predicate("red", new List<ITerm> { new Term("vase", true) }, true)));
        }
    }
}
