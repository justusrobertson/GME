using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mediation.Interfaces;
using Mediation.PlanTools;

namespace MediationTests
{
    [TestClass]
    public class PredicateTest
    {
        public Predicate testPredicate;

        public PredicateTest()
        {
            testPredicate = new Predicate("has", new List<ITerm> { new Term("?holder"), new Term("?thing") }, true);
        }

        [TestMethod]
        public void PredicateNameTest ()
        {            
            Assert.AreEqual("has", testPredicate.Name);
        }

        [TestMethod]
        public void PredicateEqualityTest()
        {
            Predicate secondPredicate = new Predicate("has", new List<ITerm> { new Term("?holder"), new Term("?thing") }, true);
            Assert.AreEqual(testPredicate, secondPredicate);

            Predicate thirdPredicate = new Predicate("has", new List<ITerm> { new Term("Justus", true), new Term("?thing") }, true);
            Assert.AreNotEqual(testPredicate, thirdPredicate);
        }

        [TestMethod]
        public void PredicateEqualityFalseTest()
        {
            Predicate secondPredicate = new Predicate("has", new List<ITerm> { new Term("?holder"), new Term("?thing") }, false);
            Assert.AreNotEqual(testPredicate, secondPredicate);
        }
    }
}
