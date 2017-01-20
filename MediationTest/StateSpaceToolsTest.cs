using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mediation.Enums;
using Mediation.PlanTools;
using Mediation.Interfaces;
using Mediation.StateSpace;

namespace MediationTest
{
    [TestClass]
    public class StateSpaceToolsTest
    {
        public Domain testDomain;
        public Problem testProblem;
        public Plan testPlan;
        public State testState;

        public StateSpaceToolsTest()
        {
            testDomain = new Domain();
            testDomain.Name = "UNIT-TEST";
            testDomain.Type = PlanType.StateSpace;
            testDomain.Operators = new List<IOperator> 
            {
                new Operator 
                    (
                        new Predicate ("move", new List<ITerm> { new Term("?mover"), new Term("?oldloction"), new Term("?loction") }, true),
                        new List<IPredicate> 
                        {
                            new Predicate ("at", new List<ITerm> { new Term("?mover"), new Term("?oldloction") }, true),
                            new Predicate ("at", new List<ITerm> { new Term("?mover"), new Term("?loction") }, false),
                            new Predicate ("character", new List<ITerm> { new Term("?mover") }, true),
                            new Predicate ("alive", new List<ITerm> { new Term("?mover") }, true),
                            new Predicate ("location", new List<ITerm> { new Term("?oldloction") }, true),
                            new Predicate ("location", new List<ITerm> { new Term("?loction") }, true),
                            new Predicate ("connected", new List<ITerm> { new Term("?loction"), new Term("?oldloction") }, true),
                        },
                        new List<IPredicate>
                        {
                            new Predicate ("at", new List<ITerm> { new Term("?mover"), new Term("?oldloction") }, false),
                            new Predicate ("at", new List<ITerm> { new Term("?mover"), new Term("?loction") }, true),
                        }
                    ),
                new Operator 
                    (
                        new Predicate ("take", new List<ITerm> { new Term("?taker"), new Term("?thing"), new Term("?place") }, true),
                        new List<IPredicate> 
                        {
                            new Predicate ("character", new List<ITerm> { new Term("?taker") }, true),
                            new Predicate ("character", new List<ITerm> { new Term("?thing") }, false),
                            new Predicate ("alive", new List<ITerm> { new Term("?taker") }, true),
                            new Predicate ("at", new List<ITerm> { new Term("?taker"), new Term("?place") }, true),
                            new Predicate ("at", new List<ITerm> { new Term("?thing"), new Term("?place") }, true),
                        },
                        new List<IPredicate>
                        {
                            new Predicate ("at", new List<ITerm> { new Term("?thing"), new Term("?place") }, false),
                            new Predicate ("has", new List<ITerm> { new Term("?taker"), new Term("?thing") }, true),
                        }
                    )
            };

            testProblem = new Problem();
            testProblem.Name = "01";
            testProblem.OriginalName = "01";
            testProblem.Domain = "UNIT-TEST";
            testProblem.Player = "arthur";
            testProblem.Objects = new List<IObject> 
            { 
                new Obj("arthur", ""), 
                new Obj("excalibur", ""), 
                new Obj("woods", ""), 
                new Obj("lake", ""), 
                new Obj("hand", ""), 
                new Obj("sam", "")
            };
            testProblem.Initial = new List<IPredicate>
            {
                new Predicate ("player", new List<ITerm> { new Term("arthur", true) }, true),
                new Predicate ("character", new List<ITerm> { new Term("arthur", true) }, true),
                new Predicate ("character", new List<ITerm> { new Term("sam", true) }, true),
                new Predicate ("alive", new List<ITerm> { new Term("sam", true) }, true),
                new Predicate ("at", new List<ITerm> { new Term("sam", true), new Term("woods", true) }, true),
                new Predicate ("alive", new List<ITerm> { new Term("arthur", true) }, true),
                new Predicate ("at", new List<ITerm> { new Term("arthur", true), new Term("woods", true) }, true),
                new Predicate ("location", new List<ITerm> { new Term("woods", true) }, true),
                new Predicate ("connected", new List<ITerm> { new Term("woods", true), new Term("lake", true) }, true),
                new Predicate ("location", new List<ITerm> { new Term("lake", true) }, true),
                new Predicate ("connected", new List<ITerm> { new Term("lake", true), new Term("woods", true) }, true),
                new Predicate ("sword", new List<ITerm> { new Term("excalibur", true) }, true),
                new Predicate ("at", new List<ITerm> { new Term("excalibur", true), new Term("lake", true) }, true)
            };
            testProblem.Goal = new List<IPredicate> { new Predicate("has", new List<ITerm> { new Term("arthur", true), new Term("excalibur", true) }, true) };

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
                new Predicate ("character", new List<ITerm> { new Term("sam", true) }, true),
                new Predicate ("alive", new List<ITerm> { new Term("sam", true) }, true),
                new Predicate ("at", new List<ITerm> { new Term("sam", true), new Term("woods", true) }, true),
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

            testState = testPlan.Initial as State;
        }

        [TestMethod]
        public void StateSpaceToolsGetAllPlayerActionsCountTest()
        {
            Assert.AreEqual(72, StateSpaceTools.GetAllPlayerActions(testDomain, testProblem).Count);
        }

        [TestMethod]
        public void StateSpaceToolsGetPlayerActionsTest()
        {
            List<Operator> testPlayerActions = new List<Operator>
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
                    )
            };
            List<Operator> computedPlayerActions = StateSpaceTools.GetPlayerActions(testDomain, testProblem, testState);

            Assert.AreEqual(testPlayerActions[0], computedPlayerActions[0]);
        }

        [TestMethod]
        public void StateSpaceToolsGetPlayerActionsCountTest()
        {
            List<Operator> testPlayerActions = new List<Operator>
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
                    )
            };

            Assert.AreEqual(testPlayerActions.Count, StateSpaceTools.GetPlayerActions(testDomain, testProblem, testState).Count);
        }

        [TestMethod]
        public void StateSpaceToolsGetPlayerActionsCountUpdatePlanTest()
        {
            Plan newPlan = testPlan.Clone() as Plan;
            State newState = testPlan.Initial.Clone() as State;
            newPlan.Initial = newState.NewState(testPlan.Steps[0] as Operator, testProblem.Objects);
            newPlan.Steps.RemoveAt(0);
            List<Operator> actions = StateSpaceTools.GetPlayerActions(testDomain, testProblem, newPlan.Initial as State);
            Assert.AreEqual(2, actions.Count);
        }

        [TestMethod]
        public void StateSpaceToolsGetSpanningLinksCountTest()
        {
            Assert.AreEqual(11, StateSpaceTools.GetSpanningLinks(testPlan).Count);
        }

        [TestMethod]
        public void StateSpaceToolsGetExceptionalActionTest()
        {
            Assert.AreEqual(1, StateSpaceTools.GetExceptionalActions(testDomain, testProblem, testPlan, testState).Count);
            Plan newPlan = testPlan.Clone() as Plan;
            State newState = testPlan.Initial.Clone() as State;
            newPlan.Initial = newState.NewState(testPlan.Steps[0] as Operator, testProblem.Objects);
            newPlan.Steps.RemoveAt(0);
            List<StateSpaceEdge> actions = StateSpaceTools.GetExceptionalActions(testDomain, testProblem, newPlan, newPlan.Initial as State);
            Assert.AreEqual(2, actions.Count);
        }

        [TestMethod]
        public void StateSpaceToolsGetConstituentActionTest()
        {
            string constituentActionName = StateSpaceTools.GetConstituentAction(testDomain, testProblem, testPlan, testState).Action.Name;
            Assert.AreEqual("move", constituentActionName);
        }

        [TestMethod]
        public void StateSpaceToolsGetLaterConstituentActionTest()
        {
            Plan newPlan = testPlan.Clone() as Plan;
            newPlan.Steps = new List<IOperator> 
            {
                new Operator 
                    (
                        new Predicate ("move", new List<ITerm> { new Term("sam", true), new Term("woods", true), new Term("lake", true) }, true),
                        new List<IPredicate> 
                        {
                            new Predicate ("at", new List<ITerm> { new Term("sam", true), new Term("woods", true) }, true),
                            new Predicate ("at", new List<ITerm> { new Term("sam", true), new Term("lake", true) }, false),
                            new Predicate ("character", new List<ITerm> { new Term("sam", true) }, true),
                            new Predicate ("alive", new List<ITerm> { new Term("sam", true) }, true),
                            new Predicate ("location", new List<ITerm> { new Term("woods", true) }, true),
                            new Predicate ("location", new List<ITerm> { new Term("lake", true) }, true),
                            new Predicate ("connected", new List<ITerm> { new Term("lake", true), new Term("woods", true) }, true),
                        },
                        new List<IPredicate>
                        {
                            new Predicate ("at", new List<ITerm> { new Term("sam", true), new Term("woods", true) }, false),
                            new Predicate ("at", new List<ITerm> { new Term("sam", true), new Term("lake", true) }, true),
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

            Assert.AreEqual("move", StateSpaceTools.GetConstituentAction(testDomain, testProblem, newPlan, testState).Action.Name);
        }

        [TestMethod]
        public void StateSpaceToolsGetPossibleActionsTest()
        {
            Assert.AreEqual(1, StateSpaceTools.GetPossibleActions(testDomain, testProblem, testPlan, testState).Count);
            Plan newPlan = testPlan.Clone() as Plan;
            State newState = testPlan.Initial.Clone() as State;
            newPlan.Initial = newState.NewState(testPlan.Steps[0] as Operator, testProblem.Objects);
            newPlan.Steps.RemoveAt(0);
            List<StateSpaceEdge> actions = StateSpaceTools.GetPossibleActions(testDomain, testProblem, newPlan, newPlan.Initial as State);
            Assert.AreEqual(2, actions.Count);
        }

        [TestMethod]
        public void StateSpaceToolsGetAllPossibleActionsTest()
        {
            Assert.AreEqual(1, StateSpaceTools.GetAllPossibleActions(testDomain, testProblem, testPlan, testState).Count);
            Plan newPlan = testPlan.Clone() as Plan;
            State newState = testPlan.Initial.Clone() as State;
            newPlan.Initial = newState.NewState(testPlan.Steps[0] as Operator, testProblem.Objects);
            newPlan.Steps.RemoveAt(0);
            newPlan.Initial.Table.Add(new Predicate("at", new List<ITerm> { new Term("hand", true), new Term("lake", true) }, true), true);
            List<StateSpaceEdge> actions = StateSpaceTools.GetAllPossibleActions(testDomain, testProblem, newPlan, newPlan.Initial as State);
            Assert.AreEqual(3, actions.Count);
        }
    }
}
