using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mediation.Interfaces;
using Mediation.PlanTools;
using Mediation.StateSpace;

namespace Mediation.Tests
{
    [TestClass]
    public class PlanSimulatorTest
    {
        public Plan testPlan;

        public PlanSimulatorTest ()
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
        public void PlanSimulatorVerifyPlanTest ()
        {
            Assert.IsTrue(PlanSimulator.VerifyPlan(testPlan, testPlan.Initial as State, new List<IObject> 
            { 
                new Obj("arthur", ""), 
                new Obj("woods", ""), 
                new Obj("lake", ""), 
                new Obj("excalibur", "") 
            }));
        }

        [TestMethod]
        public void PlanSimulatorVerifyPlanTestFalse ()
        {
            Plan newPlan = testPlan.Clone() as Plan;
            newPlan.Steps = new List<IOperator> 
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
                            new Predicate ("connected", new List<ITerm> { new Term("lake", true), new Term("woods", true) }, true),
                        },
                        new List<IPredicate>
                        {
                            new Predicate ("at", new List<ITerm> { new Term("arthur", true), new Term("woods", true) }, false),
                            new Predicate ("at", new List<ITerm> { new Term("arthur", true), new Term("lake", true) }, true),
                        }
                    ),
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
                            new Predicate ("connected", new List<ITerm> { new Term("lake", true), new Term("woods", true) }, true),
                        },
                        new List<IPredicate>
                        {
                            new Predicate ("at", new List<ITerm> { new Term("arthur", true), new Term("woods", true) }, false),
                            new Predicate ("at", new List<ITerm> { new Term("arthur", true), new Term("lake", true) }, true),
                        }
                    )
            };
            Assert.IsFalse(PlanSimulator.VerifyPlan(newPlan, newPlan.Initial as State, new List<IObject> 
            { 
                new Obj("arthur", ""), 
                new Obj("woods", ""), 
                new Obj("lake", ""), 
                new Obj("excalibur", "") 
            }));
        }
    }
}
