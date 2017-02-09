using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mediation.Interfaces;
using Mediation.PlanTools;

namespace Mediation.Tests
{
    [TestClass]
    public class StateTest
    {
        public State testState;
        public Operator action;

        public StateTest()
        {
            testState = new State();
            testState.Predicates = new List<IPredicate>
            {
                new Predicate ("at", new List<ITerm> { new Term("arthur", true), new Term("woods", true) }, true),
                new Predicate ("character", new List<ITerm> { new Term("arthur", true) }, true),
                new Predicate ("alive", new List<ITerm> { new Term("arthur", true) }, true),
                new Predicate ("location", new List<ITerm> { new Term("woods", true) }, true),
                new Predicate ("location", new List<ITerm> { new Term("lake", true) }, true),
                new Predicate ("connected", new List<ITerm> { new Term("lake", true), new Term("woods", true) }, true),
                new Predicate ("at", new List<ITerm> { new Term("excalibur", true), new Term("lake", true) }, true)
            };
            action = new Operator
                (
                    new Predicate("move", new List<ITerm> { new Term("arthur", true), new Term("woods", true), new Term("lake", true) }, true),
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
                );
            action.Conditionals.Add
                (
                    new Axiom
                        (
                            new List<ITerm> { new Term("?x") },
                            new List<IPredicate> 
                            {
                                new Predicate ("alive", new List<ITerm> { new Term ("?x") }, true),
                                new Predicate ("at", new List<ITerm> { new Term ("?x"), new Term ("?oldlocation") }, true)
                            },
                            new List<IPredicate>
                            {
                                new Predicate ("fabulous", new List<ITerm> { new Term ("?x") }, true)
                            },
                            new Hashtable()
                        )
                );
            Hashtable binds = new Hashtable();
            binds.Add("?oldlocation", "woods");
            action.Bindings = binds;
        }

        [TestMethod]
        public void StateInStateMethodTest()
        {
            Assert.IsTrue(testState.InState(new Predicate("at", new List<ITerm> { new Term ("arthur", true), new Term ("woods", true) }, true)));
        }

        [TestMethod]
        public void StateInStateMethodTestFalse()
        {
            Assert.IsFalse(testState.InState(new Predicate("at", new List<ITerm> { new Term ("arthur", true), new Term ("lake", true) }, true)));
            Assert.IsFalse(testState.InState(new Predicate("at", new List<ITerm> { new Term ("arthur", true), new Term ("woods", true) }, false)));
        }

        [TestMethod]
        public void StateSatisfiesMethodTest()
        {
            Assert.IsTrue(testState.Satisfies(new List<IPredicate>
            {
                new Predicate("at", new List<ITerm> { new Term ("arthur", true), new Term ("woods", true) }, true),
                new Predicate("character", new List<ITerm> { new Term ("arthur", true) }, true),
                new Predicate("alive", new List<ITerm> { new Term ("arthur", true) }, true),
                new Predicate("location", new List<ITerm> { new Term ("woods", true) }, true),
                new Predicate("location", new List<ITerm> { new Term ("lake", true) }, true)
            }));
        }

        [TestMethod]
        public void StateSatisfiesMethodTestFalse()
        {
            Assert.IsFalse(testState.Satisfies(new List<IPredicate>
            {
                new Predicate("at", new List<ITerm> { new Term ("arthur", true), new Term ("lake", true) }, true)
            }));
        }

        [TestMethod]
        public void StateEqualsTest()
        {
            State testState2 = new State();
            testState2.Predicates = new List<IPredicate>
            {
                new Predicate("at", new List<ITerm> { new Term ("arthur", true), new Term ("woods", true) }, true),
                new Predicate("character", new List<ITerm> { new Term ("arthur", true) }, true),
                new Predicate("alive", new List<ITerm> { new Term ("arthur", true) }, true),
                new Predicate("location", new List<ITerm> { new Term ("woods", true) }, true),
                new Predicate("location", new List<ITerm> { new Term ("lake", true) }, true),
                new Predicate("connected", new List<ITerm> { new Term ("lake", true), new Term ("woods", true) }, true),
                new Predicate("at", new List<ITerm> { new Term ("excalibur", true), new Term ("lake", true) }, true)
            };

            Assert.AreEqual(testState, testState2);
        }

        [TestMethod]
        public void StateApplyActionTest()
        {
            Operator action = new Operator
                (
                    new Predicate("move", new List<ITerm> { new Term("arthur", true), new Term("woods", true), new Term("lake", true) }, true),
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
                );
            List<IPredicate> preds = new List<IPredicate>
            {
                new Predicate ("character", new List<ITerm> { new Term("arthur", true) }, true),
                new Predicate ("alive", new List<ITerm> { new Term("arthur", true) }, true),
                new Predicate ("at", new List<ITerm> { new Term("arthur", true), new Term("lake", true) }, true),
                new Predicate ("location", new List<ITerm> { new Term("woods", true) }, true),
                new Predicate ("location", new List<ITerm> { new Term("lake", true) }, true),
                new Predicate ("connected", new List<ITerm> { new Term("lake", true), new Term("woods", true) }, true),
                new Predicate ("at", new List<ITerm> { new Term("excalibur", true), new Term("lake", true) }, true)
            };
            List<IPredicate> actualPreds = testState.ApplyAction(action, new List<IObject> 
            { 
                new Obj("arthur", ""), 
                new Obj("woods", ""), 
                new Obj("lake", ""), 
                new Obj("excalibur", "") 
            });

            foreach (Predicate pred in preds)
                Assert.IsTrue(actualPreds.Contains(pred));

            foreach (Predicate pred in actualPreds)
                Assert.IsTrue(preds.Contains(pred));
        }

        [TestMethod]
        public void StateApplyActionTestConditional ()
        {
            List<IPredicate> preds = new List<IPredicate>
            {
                new Predicate("character", new List<ITerm> { new Term("arthur", true) }, true),
                new Predicate("alive", new List<ITerm> { new Term("arthur", true) }, true),
                new Predicate("location", new List<ITerm> { new Term("woods", true) }, true),
                new Predicate("location", new List<ITerm> { new Term("lake", true) }, true),
                new Predicate("connected", new List<ITerm> { new Term("lake", true), new Term("woods", true) }, true),
                new Predicate ("at", new List<ITerm> { new Term("arthur", true), new Term("lake", true) }, true),
                new Predicate("at", new List<ITerm> { new Term("excalibur", true), new Term("lake", true) }, true),
                new Predicate("fabulous", new List<ITerm> { new Term("?x", "arthur") }, true)
            };
            List<IPredicate> actualPreds = testState.ApplyAction(action, new List<IObject> 
            { 
                new Obj("arthur", ""), 
                new Obj("woods", ""), 
                new Obj("lake", ""), 
                new Obj("excalibur", "") 
            });

            foreach (Predicate pred in preds)
                Assert.IsTrue(actualPreds.Contains(pred));

            foreach (Predicate pred in actualPreds)
                Assert.IsTrue(preds.Contains(pred));
        }

        [TestMethod]
        public void StateNewStateTest()
        {
            State state = new State
            (
                new List<IPredicate>
                {
                    new Predicate("character", new List<ITerm> { new Term("arthur", true) }, true),
                    new Predicate("alive", new List<ITerm> { new Term("arthur", true) }, true),
                    new Predicate("location", new List<ITerm> { new Term("woods", true) }, true),
                    new Predicate("location", new List<ITerm> { new Term("lake", true) }, true),
                    new Predicate("connected", new List<ITerm> { new Term("lake", true), new Term("woods", true) }, true),
                    new Predicate ("at", new List<ITerm> { new Term("arthur", true), new Term("lake", true) }, true),
                    new Predicate("at", new List<ITerm> { new Term("excalibur", true), new Term("lake", true) }, true)
                },
                new Operator(),
                new Operator()
            );
            Operator action = new Operator
            (
                new Predicate("move", new List<ITerm> { new Term("arthur", true), new Term("woods", true), new Term("lake", true) }, true),
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
            );
            State newState = testState.NewState(action, new List<IObject> 
            { 
                new Obj("arthur", ""), 
                new Obj("woods", ""), 
                new Obj("lake", ""), 
                new Obj("excalibur", "") 
            });
            Assert.AreEqual(state, newState);
        }
    }
}
