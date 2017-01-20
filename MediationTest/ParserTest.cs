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

namespace MediationTest
{
    [TestClass]
    public class ParserTest
    {
        string testDomainName;
        string testDomainDirectory;
        Domain testDomain;
        Problem testProblem;
        Plan testPlan;

        public ParserTest ()
        {
            testDomainName = "unit-test";
            testDomainDirectory = Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl";
            testDomain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl", PlanType.PlanSpace);
            testProblem = Parser.GetProblem(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\prob01.pddl");
            testPlan = FastDownward.Plan(testDomain, testProblem);
        }

        [TestMethod]
        public void ParserTopDirectoryTest()
        {
            Assert.IsTrue(File.Exists(testDomainDirectory));
        }

        [TestMethod]
        public void ParsedDomainNameTest()
        {
            Assert.AreEqual("UNIT-TEST", testDomain.Name);
        }

        [TestMethod]
        public void ParsedDomainOperatorNameTest()
        {
            IOperator testActionTemplate = testDomain.Operators[0];
            Assert.AreEqual("move", testActionTemplate.Name);

            testActionTemplate = testDomain.Operators[1];
            Assert.AreEqual("take", testActionTemplate.Name);
        }

        [TestMethod]
        public void ParsedDomainOperatorConditionalEffectTest()
        {
            IOperator testActionTemplate = testDomain.Operators[0];
            Assert.AreEqual("move", testActionTemplate.Name);
            Assert.AreEqual(1, testActionTemplate.Conditionals.Count);
        }

        [TestMethod]
        public void ParsedDomainOperatorPreconditionTest()
        {
            IOperator testActionTemplate = testDomain.Operators[0];
            IPredicate testPrecondition = testActionTemplate.Preconditions[0];
            Assert.AreEqual("character", testPrecondition.Name);
            Assert.AreEqual(new Term("?mover"), testPrecondition.TermAt(0));
        }

        [TestMethod]
        public void ParsedProblemNameTest()
        {
            Assert.AreEqual("UNIT-TEST", testProblem.Domain);
        }

        [TestMethod]
        public void ParsedPlanStepCountTest()
        {
            Assert.AreEqual(2, testPlan.Steps.Count);
        }
    }
}
