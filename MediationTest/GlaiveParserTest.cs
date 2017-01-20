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
    public class GlaiveParserTest
    {
        string testDomainName;
        string testDomainDirectory;
        Domain testDomain;
        Problem testProblem;
        Plan testPlan;

        public GlaiveParserTest ()
        {
            testDomainName = "arth";
            testDomainDirectory = Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl";
            testDomain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl", PlanType.StateSpace);
            testProblem = Parser.GetProblem(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\prob01.pddl");
            testPlan = Glaive.Plan(testDomain, testProblem);
        }

        [TestMethod]
        public void GlaiveParserTopDirectoryTest()
        {
            Assert.IsTrue(File.Exists(testDomainDirectory));
        }

        [TestMethod]
        public void GlaiveParsedDomainNameTest()
        {
            Assert.AreEqual("arth", testDomain.Name);
        }

        [TestMethod]
        public void GlaiveParsedDomainTypesTest()
        {
            Assert.AreEqual(3, testDomain.ObjectTypes.Count);
        }

        [TestMethod]
        public void GlaiveParsedDomainConstantsTest()
        {
            Assert.AreEqual(0, testDomain.ConstantTypes.Count);
        }

        [TestMethod]
        public void GlaiveParsedDomainOperatorNameTest()
        {
            IOperator testActionTemplate = testDomain.Operators[0];
            Assert.AreEqual("move-location", testActionTemplate.Name);

            testActionTemplate = testDomain.Operators[1];
            Assert.AreEqual("wake-person", testActionTemplate.Name);
        }

        [TestMethod]
        public void GlaiveParsedDomainParametersTest()
        {
            IOperator testActionTemplate = testDomain.Operators[0];
            Assert.AreEqual("?character", testActionTemplate.Predicate.TermAt(0).Variable);
            Assert.AreEqual("character", testActionTemplate.Predicate.TermAt(0).Type);
        }

        [TestMethod]
        public void GlaiveParsedDomainPreconditionTest()
        {
            IOperator testActionTemplate = testDomain.Operators[0];
            Assert.AreEqual("connected", testActionTemplate.Preconditions[0].Name);
            Assert.AreEqual("?to", testActionTemplate.Preconditions[0].TermAt(0).Variable);
            Assert.AreEqual("location", testActionTemplate.Preconditions[0].TermAt(0).Type);
        }

        [TestMethod]
        public void GlaiveParsedDomainEffectTest()
        {
            IOperator testActionTemplate = testDomain.Operators[0];
            Assert.AreEqual("at", testActionTemplate.Effects[0].Name);
            Assert.IsFalse(testActionTemplate.Effects[0].Sign);
            Assert.AreEqual("?character", testActionTemplate.Effects[0].TermAt(0).Variable);
            Assert.AreEqual("character", testActionTemplate.Effects[0].TermAt(0).Type);
            Assert.AreEqual(2, testActionTemplate.Effects[1].Terms.Count);
        }

        [TestMethod]
        public void GlaiveParsedDomainConsentingAgentTest()
        {
            testDomain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl", PlanType.StateSpace);
            IOperator testActionTemplate = testDomain.Operators[0];
            Assert.AreEqual(1, testActionTemplate.ConsentingAgents.Count);
            Assert.AreEqual("?character", testActionTemplate.ConsentingAgents[0].Variable);
        }

        [TestMethod]
        public void GlaiveParsedProblemObjectsTest()
        {
            Assert.AreEqual(testProblem.Objects.Count, 9);
        }

        [TestMethod]
        public void GlaiveParsedProblemIntentionsTest()
        {
            Assert.AreEqual(testProblem.Intentions.Count, 2);
        }

        [TestMethod]
        public void GlaiveParsedProblemIntentionsContentTest()
        {
            IIntention intention = testProblem.Intentions[0];
            Assert.AreEqual("arthur", intention.Character);
            Assert.AreEqual("has", intention.Predicate.Name);
            Assert.AreEqual("arthur", intention.Predicate.TermAt(0).Constant);
            Assert.AreEqual("excalibur", intention.Predicate.TermAt(1).Constant);
        }

        [TestMethod]
        public void GlaiveParsedProblemGoalsTest()
        {
            testProblem = Parser.GetProblem(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\prob01.pddl");
            Assert.AreEqual(testProblem.Goal.Count, 1);
            Assert.AreEqual(testProblem.Goal[0].Name, "has");
            Assert.AreEqual(testProblem.Goal[0].Terms.Count, 2);
        }

        [TestMethod]
        public void GlaiveParsedPlanTest()
        {
            Assert.AreEqual(testPlan.Steps.Count, 4);
            Assert.AreEqual(testPlan.Steps[0].Name, "take-thing");
        }
    }
}
