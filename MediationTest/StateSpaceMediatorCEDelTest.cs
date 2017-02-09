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
    public class StateSpaceMediatorCEDelTest
    {
        string testDomainName;
        string testDomainDirectory;
        Domain testDomain;
        Problem testProblem;
        Plan testPlan;

        public StateSpaceMediatorCEDelTest ()
        {
            testDomainName = "cedel-test";
            testDomainDirectory = Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl";
            testDomain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl", PlanType.StateSpace);
            testProblem = Parser.GetProblem(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\prob01.pddl");
            testPlan = FastDownward.Plan(testDomain, testProblem);
        }

        [TestMethod]
        public void StateSpaceMediatorExampleCEParseTest()
        {
            StateSpaceNode root = StateSpaceMediator.BuildTree(Planner.FastDownward, testDomain, testProblem, testPlan, testPlan.Initial as State, 3);
            StateSpaceNode child = root.children[root.outgoing[1]] as StateSpaceNode;
            Assert.IsFalse(child.incoming.Action.Effects.Contains(new Predicate("when", new List<ITerm> { }, true)));
        }

        [TestMethod]
        public void StateSpaceMediatorExampleCEDeleteTest()
        {
            StateSpaceMediator.CEDeletion = true;
            StateSpaceNode root = StateSpaceMediator.BuildTree(Planner.FastDownward, testDomain, testProblem, testPlan, testPlan.Initial as State, 3);
            StateSpaceNode child = root.children[root.outgoing[2]] as StateSpaceNode;
            Assert.IsTrue(child.incoming.Action.Conditionals.Count == 0);
        }

        [TestMethod]
        public void StateSpaceMediatorExampleStateUpdateTest()
        {
            StateSpaceMediator.CEDeletion = true;
            StateSpaceNode root = StateSpaceMediator.BuildTree(Planner.FastDownward, testDomain, testProblem, testPlan, testPlan.Initial as State, 3);
            StateSpaceNode child = root.children[root.outgoing[2]] as StateSpaceNode;
            Assert.IsTrue(child.Goal);
        }
    }
}
