using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mediation.PlanTools;
using Mediation.Interfaces;

namespace MediationTest
{
    [TestClass]
    public class PlanTest
    {
        public Plan testPlan;

        public PlanTest()
        {
            testPlan = new Plan();
            testPlan.Steps = new List<IOperator> 
            {
                new Operator 
                    (
                        new Predicate ("move", new List<ITerm> { new Term("arthur", true), new Term("woods", true), new Term("lake", true) }, true),
                        new List<IPredicate> 
                        {
                            new Predicate ("at", new List<ITerm> { new Term("arthur", true), new Term("woods", true) }, true),
                            new Predicate ("at", new List<ITerm> { new Term("arthur", true), new Term("lake", true) }, false),
                            new Predicate ("character", new List<ITerm> { new Term("arthur", true) }, true),
                            new Predicate ("alive", new List<ITerm> { new Term("arthur", true) }, true),
                            new Predicate ("location", new List<ITerm> { new Term("woods", true) }, true),
                            new Predicate ("location", new List<ITerm> { new Term("lake", true) }, true),
                            new Predicate ("connected", new List<ITerm> { new Term("woods", true), new Term("lake", true) }, true),
                        },
                        new List<IPredicate>
                        {
                            new Predicate ("at", new List<ITerm> { new Term("arthur", true), new Term("woods", true) }, false),
                            new Predicate ("at", new List<ITerm> { new Term("arthur", true), new Term("lake", true) }, true),
                        }
                    ),
                new Operator 
                    (
                        new Predicate ("take", new List<ITerm> { new Term("arthur", true), new Term("excalibur", true), new Term("lake", true) }, true),
                        new List<IPredicate> 
                        {
                            new Predicate ("character", new List<ITerm> { new Term("arthur", true) }, true),
                            new Predicate ("character", new List<ITerm> { new Term("excalibur", true) }, false),
                            new Predicate ("alive", new List<ITerm> { new Term("arthur", true) }, true),
                            new Predicate ("at", new List<ITerm> { new Term("arthur", true), new Term("lake", true) }, true),
                            new Predicate ("at", new List<ITerm> { new Term("excalibur", true), new Term("lake", true) }, true),
                        },
                        new List<IPredicate>
                        {
                            new Predicate ("at", new List<ITerm> { new Term("excalibur", true), new Term("lake", true) }, false),
                            new Predicate ("has", new List<ITerm> { new Term("arthur", true), new Term("excalibur", true) }, true),
                        }
                    )
            };
            testPlan.Initial.Predicates = new List<IPredicate>
            {
                new Predicate ("player", new List<ITerm> { new Term("arthur", true) }, true),
                new Predicate ("character", new List<ITerm> { new Term("arthur", true) }, true),
                new Predicate ("alive", new List<ITerm> { new Term("arthur", true) }, true),
                new Predicate ("at", new List<ITerm> { new Term("arthur", true), new Term("woods", true) }, true),
                new Predicate ("location", new List<ITerm> { new Term("woods", true) }, true),
                new Predicate ("connected", new List<ITerm> { new Term("woods", true), new Term("lake", true) }, true),
                new Predicate ("location", new List<ITerm> { new Term("lake", true) }, true),
                new Predicate ("connected", new List<ITerm> { new Term("lake", true), new Term("woods", true) }, true),
                new Predicate ("sword", new List<ITerm> { new Term("excalibur", true) }, true),
                new Predicate ("at", new List<ITerm> { new Term("excalibur", true), new Term("lake", true) }, true)
            };
            testPlan.Goal.Predicates = new List<IPredicate> { new Predicate("has", new List<ITerm> { new Term("arthur", true), new Term("excalibur", true) }, true) };
        }

        [TestMethod]
        public void PlanInitialStateTest()
        {
            Assert.AreEqual(10, testPlan.Initial.Table.Keys.Count);
        }

        [TestMethod]
        public void PlanInitialStateStepEffectsTest()
        {
            Assert.AreEqual(10, testPlan.InitialStep.Effects.Count);
        }

        [TestMethod]
        public void PlanGoalStatePreconditionsTest()
        {
            Assert.AreEqual(1, testPlan.GoalStep.Preconditions.Count);
        }

        [TestMethod]
        public void PlanDependenciesTest()
        {
            Assert.AreEqual(13, testPlan.Dependencies.Count);
        }

        [TestMethod]
        public void PlanDependenciesSpanTest()
        {
            Assert.AreEqual(3, testPlan.Dependencies[8].Span.Count);
        }

        [TestMethod]
        public void PlanDependenciesTypeTest()
        {
            Assert.IsInstanceOfType(testPlan.Dependencies[0], typeof(CausalLink));
        }
    }
}
