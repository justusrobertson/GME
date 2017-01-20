using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mediation.Interfaces;
using Mediation.PlanTools;

namespace MediationTest
{
    [TestClass]
    public class OperatorTest
    {
        public Operator testOperator;

        public OperatorTest()
        {
            testOperator = new Operator (
                                            "take",
                                            new List<ITerm> { new Term("?taker"), new Term("?thing") },
                                            new Hashtable(),
                                            new List<IPredicate>
                                            {
                                                new Predicate ("has", new List<ITerm> { new Term("?taker"), new Term("?thing") }, false)
                                            },
                                            new List<IPredicate>
                                            {
                                                new Predicate ("has", new List<ITerm> { new Term("?taker"), new Term("?thing") }, true)
                                            }
                                        );
        }

        [TestMethod]
        public void OperatorNameTest()
        {
            Assert.AreEqual("take", testOperator.Name);
        }

        [TestMethod]
        public void OperatorTermAtNoBindingTest()
        {
            Assert.AreEqual("?taker", testOperator.TermAt(0));
        }

        /*[TestMethod]
        public void OperatorTermAtBindingTest()
        {
            Operator boundTestOperator = (Operator)testOperator.Clone();
            boundTestOperator.Bindings.Add("?taker", "sam");
            Assert.AreEqual("sam", boundTestOperator.TermAt(0));
        }*/

        [TestMethod]
        public void OperatorEqualityTest()
        {
            Operator secondOperator = new Operator
                (
                    new Predicate("take", new List<ITerm> { new Term("?taker"), new Term("?thing") }, true),
                    new List<IPredicate>
                    {
                        new Predicate ("has", new List<ITerm> { new Term("?taker"), new Term("?thing") }, false)
                    },
                    new List<IPredicate>
                    {
                        new Predicate ("has", new List<ITerm> { new Term("?taker"), new Term("?thing") }, true)
                    }
                );

            Assert.AreEqual(testOperator, secondOperator);
        }

        [TestMethod]
        public void OperatorEqualityPreconditionFalseTest()
        {
            Operator secondOperator = new Operator
                (
                    new Predicate("take", new List<ITerm> { new Term("?taker"), new Term("?thing") }, true),
                    new List<IPredicate>
                    {
                        new Predicate ("has", new List<ITerm> { new Term("?taker"), new Term("?thing") }, true)
                    },
                    new List<IPredicate>
                    {
                        new Predicate ("has", new List<ITerm> { new Term("?taker"), new Term("?thing") }, true)
                    }
                );

            Assert.AreNotEqual(testOperator, secondOperator);
        }

        [TestMethod]
        public void OperatorActorTest()
        {
            Assert.AreEqual("?taker", testOperator.Actor);
        }

        [TestMethod]
        public void OperatorIDTest()
        {
            Operator secondOperator = new Operator
                (
                    new Predicate("take", new List<ITerm> { new Term("?taker"), new Term("?thing") }, true),
                    new List<IPredicate>
                    {
                        new Predicate ("has", new List<ITerm> { new Term("?taker"), new Term("?thing") }, false)
                    },
                    new List<IPredicate>
                    {
                        new Predicate ("has", new List<ITerm> { new Term("?taker"), new Term("?thing") }, true)
                    }
                );
            Operator clone = testOperator.Clone() as Operator;

            Assert.AreNotEqual(testOperator.ID, secondOperator.ID);
            Assert.AreEqual(testOperator.ID, clone.ID);
        }

        [TestMethod]
        public void OperatorUpdateTermsTest()
        {
            Hashtable binds = new Hashtable();
            binds.Add("?taker", "frodo");
            binds.Add("?thing", "ring");
            testOperator.Bindings = binds;
            Assert.AreEqual("frodo", testOperator.TermAt(0));
            Assert.AreEqual("ring", testOperator.TermAt(1));
            Assert.AreEqual(new Term("?taker", "frodo"), testOperator.Terms[0]);
        }

        [TestMethod]
        public void OperatorUpdateBindingsTest()
        {
            List<ITerm> newTerms = new List<ITerm> { new Term("?taker", "frodo"), new Term("?thing", "ring") };
            testOperator.Terms = newTerms;
            Assert.AreEqual("frodo", testOperator.TermAt(0));
            Assert.AreEqual("ring", testOperator.TermAt(1));
            Assert.AreEqual(testOperator.Bindings["?taker"], "frodo");
        }

        [TestMethod]
        public void OperatorAddBindingTest()
        {
            testOperator.AddBinding("?taker", "frodo");
            testOperator.AddBinding("?thing", "ring");
            Assert.AreEqual("frodo", testOperator.TermAt(0));
            Assert.AreEqual("ring", testOperator.TermAt(1));
            Assert.AreEqual(testOperator.Bindings["?taker"], "frodo");
        }

        [TestMethod]
        public void OperatorConsentingAgentsTest()
        {
            testOperator.AddBinding("?taker", "frodo");
            testOperator.AddBinding("?thing", "ring");
            List<ITerm> consenting = new List<ITerm>();
            consenting.Add(new Term("?taker"));
            testOperator.ConsentingAgents = consenting;
            Assert.AreEqual("?taker", testOperator.ConsentingAgents[0].Variable);
            Assert.AreEqual("frodo", testOperator.ConsentingAgents[0].Constant);
        }
    }
}
