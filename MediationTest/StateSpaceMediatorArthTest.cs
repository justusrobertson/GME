using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mediation.Interfaces;
using Mediation.PlanTools;
using Mediation.FileIO;
using Mediation.Planners;
using Mediation.Enums;
using Mediation.StateSpace;

namespace Mediation.Tests
{
    [TestClass]
    public class StateSpaceMediatorArthTest
    {
        string testDomainName;
        string testDomainDirectory;
        Domain testDomain;
        Problem testProblem;
        Plan testPlan;

        public StateSpaceMediatorArthTest ()
        {
            testDomainName = "arth";
            testDomainDirectory = Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl";
            testDomain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl", PlanType.StateSpace);
            testProblem = Parser.GetProblemWithTypes(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\prob01.pddl", testDomain);
            testPlan = Glaive.Plan(testDomain, testProblem);
        }

        [TestMethod]
        public void StateSpaceMediatorArthActionTest()
        {
            StateSpaceNode root = StateSpaceMediator.BuildTree(Planner.Glaive, testDomain, testProblem, testPlan, testPlan.Initial as State, 3);
            Assert.AreEqual(root.outgoing.Count, 3);
        }

        [TestMethod]
        public void StateSpaceMediatorExceptionalActionTest()
        {
            StateSpaceNode root = StateSpaceMediator.BuildTree(Planner.Glaive, testDomain, testProblem, testPlan, testPlan.Initial as State, 3);
            Assert.AreEqual(root.outgoing[1].ActionType, ActionType.Exceptional);
            Assert.AreEqual(root.outgoing[1].Action.Name, "move-location");
        }

        [TestMethod]
        public void StateSpaceMediatorExceptionalUpdateIntentionTest()
        {
            StateSpaceNode root = StateSpaceMediator.BuildTree(Planner.Glaive, testDomain, testProblem, testPlan, testPlan.Initial as State, 1);
            StateSpaceNode child = root.children[root.outgoing[1]] as StateSpaceNode;
            Assert.AreEqual(child.problem.Intentions.Count, 2);
            Assert.AreNotEqual(child.plan.Steps.Count, 4);
        }
    }
}
