using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mediation.PlanTools;
using Mediation.MediationTree;
using Mediation.FileIO;
using Mediation.Enums;
using System.Collections.Generic;
using Mediation.Interfaces;

namespace MediationTest
{
    [TestClass]
    public class MediationTreeTest
    {
        string testDomainName;
        string testDomainDirectory;
        Domain testDomain;
        Problem testProblem;
        MediationTree tree;
        string pathEvent;
        string pathDomain;
        string pathSuper;

        public MediationTreeTest()
        {
            testDomainName = "spy-types";
            testDomainDirectory = Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl";
            testDomain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl", PlanType.StateSpace);
            testProblem = Parser.GetProblemWithTypes(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\prob01.pddl", testDomain);
            pathEvent = Parser.GetTopDirectory() + @"MediationTrees\Data\Unit-Tests\Spy\event\";
            pathDomain = Parser.GetTopDirectory() + @"MediationTrees\Data\Unit-Tests\Spy\domain\";
            pathSuper = Parser.GetTopDirectory() + @"MediationTrees\Data\Unit-Tests\Spy\super\";
        }

        [TestMethod]
        public void SuperpositionManipulationTest()
        {
            tree = new MediationTree(testDomain, testProblem, pathSuper, false, false, true);
            List<VirtualMediationTreeEdge> supers = new List<VirtualMediationTreeEdge>();
            foreach (MediationTreeEdge edge in tree.Root.Outgoing)
                if (edge is VirtualMediationTreeEdge) supers.Add(edge as VirtualMediationTreeEdge);
            Assert.AreEqual(supers.Count, 1);

            VirtualMediationTreeNode node = tree.GetNode(tree.Root.Domain, tree.Root.Problem, supers[0]) as VirtualMediationTreeNode;
            node = tree.GetNode(node.Domain, node.Problem, node.Outgoing.Find(x => x.Action.Name.Equals("toggle-green"))) as VirtualMediationTreeNode;
            node = tree.GetNode(node.Domain, node.Problem, node.Outgoing[0]) as VirtualMediationTreeNode;
            Assert.IsTrue(node.Outgoing.Count > 1);

            node = tree.GetNode(node.Domain, node.Problem, node.Outgoing.Find(x => x.Action.Name.Equals("toggle-red"))) as VirtualMediationTreeNode;
            node = tree.GetNode(node.Domain, node.Problem, node.Outgoing[0]) as VirtualMediationTreeNode;
            node = tree.GetNode(node.Domain, node.Problem, node.Outgoing.Find(x => x.Action.Name.Equals("toggle-green"))) as VirtualMediationTreeNode;
            node = tree.GetNode(node.Domain, node.Problem, node.Outgoing[0]) as VirtualMediationTreeNode;

            Predicate term = new Predicate("used", new List<ITerm> { new Term("terminal1", true), new Term("boss", true) }, true);
            Predicate term2 = new Predicate("used", new List<ITerm> { new Term("terminal2", true), new Term("boss", true) }, true);
            Predicate term3 = new Predicate("has", new List<ITerm> { new Term("snake", true), new Term("c4", true) }, true);
            Predicate bossGear = new Predicate("at", new List<ITerm> { new Term("boss", true), new Term("gear", true) }, true);

            Superposition super = node.State as Superposition;
            Assert.IsTrue(super.IsUndetermined(term));
            Assert.IsTrue(super.IsUndetermined(term2));
            Assert.IsFalse(super.IsUndetermined(term3));

            node = tree.GetNode(node.Domain, node.Problem, node.Outgoing.Find(x => x.Action.Name.Equals("move-location"))) as VirtualMediationTreeNode;
            super = node.State as Superposition;
            Assert.IsFalse(super.IsUndetermined(term));

            if (super.IsFalse(bossGear)) Assert.IsTrue(super.IsUndetermined(term2));
            else Assert.IsFalse(super.IsUndetermined(term2));
        }

        [TestMethod]
        public void EventRevisionTest()
        {
            tree = new MediationTree(testDomain, testProblem, pathEvent, false, true);
            MediationTreeNode child = tree.GetNode(tree.Root.Domain, tree.Root.Problem, tree.Root.Outgoing.Find(x => x.Action.Name.Equals("move-location") && x.Action.TermAt(1).Equals("right")));

            Assert.AreEqual(child.Outgoing.Count, 2);
            Assert.AreEqual(child.Outgoing.Find(x => x.Action.Name.Equals("move-location")).ActionType, ActionType.Constituent);
            Assert.AreEqual(child.Outgoing.Find(x => x.Action.Name.Equals("toggle-green")).ActionType, ActionType.Consistent);

            child = tree.GetNode(child.Domain, child.Problem, child.Outgoing.Find(x => x.Action.Name.Equals("move-location")));
            child = tree.GetNode(child.Domain, child.Problem, child.Outgoing.Find(x => x.Action.Name.Equals("make-trap-wire")));

            Assert.AreEqual(child.Outgoing.Find(x => x.Action.Name.Equals("place-explosive-thing") && x.Action.TermAt(2).Equals("gears")).ActionType, ActionType.Constituent);

            child = tree.GetNode(child.Domain, child.Problem, child.Outgoing.Find(x => x.Action.Name.Equals("place-explosive-thing") && x.Action.TermAt(2).Equals("gears")));
            child = tree.GetNode(child.Domain, child.Problem, child.Outgoing.Find(x => x.Action.Name.Equals("set-trap")));

            Assert.AreEqual(child.Outgoing.Find(x => x.Action.Name.Equals("move-location") && x.Action.TermAt(1).Equals("right")).ActionType, ActionType.Constituent);
            child = tree.GetNode(child.Domain, child.Problem, child.Outgoing.Find(x => x.Action.Name.Equals("move-location") && x.Action.TermAt(1).Equals("left")));
            Assert.IsFalse(child.DeadEnd);
            Assert.AreEqual(child.Plan.Steps.Count, 7);
        }

        [TestMethod]
        public void DomainRevisionTest()
        {
            tree = new MediationTree(testDomain, testProblem, pathDomain, true, false);
            MediationTreeNode child = tree.GetNode(tree.Root.Domain, tree.Root.Problem, tree.Root.Outgoing.Find(x => x.Action.Name.Equals("move-location") && x.Action.TermAt(1).Equals("right")));

            Assert.AreEqual(child.Outgoing.Count, 2);
            Assert.AreEqual(child.Outgoing.Find(x => x.Action.Name.Equals("move-location")).ActionType, ActionType.Constituent);
            Assert.AreEqual(child.Outgoing.Find(x => x.Action.Name.Equals("toggle-green")).ActionType, ActionType.Consistent);
            Assert.AreEqual(child.Domain.Operators.Find(x => x.Name.Equals("detonate-explosive")).Conditionals.Count, 2);

            child = tree.GetNode(child.Domain, child.Problem, child.Outgoing.Find(x => x.Action.Name.Equals("move-location")));
            child = tree.GetNode(child.Domain, child.Problem, child.Outgoing.Find(x => x.Action.Name.Equals("make-trap-wire")));

            Assert.AreEqual(child.Outgoing.Find(x => x.Action.Name.Equals("place-explosive-thing") && x.Action.TermAt(2).Equals("gears")).ActionType, ActionType.Constituent);

            child = tree.GetNode(child.Domain, child.Problem, child.Outgoing.Find(x => x.Action.Name.Equals("place-explosive-thing") && x.Action.TermAt(2).Equals("terminal1")));
            child = tree.GetNode(child.Domain, child.Problem, child.Outgoing.Find(x => x.Action.Name.Equals("set-trap")));
            child = tree.GetNode(child.Domain, child.Problem, child.Outgoing.Find(x => x.Action.Name.Equals("detonate-explosive")));
            Assert.IsFalse(child.DeadEnd);
            Assert.AreEqual(child.Domain.Operators.Find(x => x.Name.Equals("detonate-explosive")).Conditionals.Count, 1);
            Assert.AreEqual(child.Domain.Operators.Find(x => x.Name.Equals("detonate-explosive")).Conditionals[0].Preconditions[0].Name, "green");

            child = tree.GetNode(child.Domain, child.Problem, child.Outgoing.Find(x => x.Action.Name.Equals("move-location")));
            child = tree.GetNode(child.Domain, child.Problem, child.Outgoing.Find(x => x.Action.Name.Equals("take-explosive")));
            child = tree.GetNode(child.Domain, child.Problem, child.Outgoing.Find(x => x.Action.Name.Equals("use-computer")));
            child = tree.GetNode(child.Domain, child.Problem, child.Outgoing.Find(x => x.Action.Name.Equals("toggle-green")));
            child = tree.GetNode(child.Domain, child.Problem, child.Outgoing.Find(x => x.Action.Name.Equals("link-phone")));
            child = tree.GetNode(child.Domain, child.Problem, child.Outgoing.Find(x => x.Action.Name.Equals("place-explosive-thing") && x.Action.TermAt(2).Equals("terminal1")));
            child = tree.GetNode(child.Domain, child.Problem, child.Outgoing.Find(x => x.Action.Name.Equals("do-nothing")));
            child = tree.GetNode(child.Domain, child.Problem, child.Outgoing.Find(x => x.Action.Name.Equals("detonate-explosive")));
            Assert.IsTrue(child.DeadEnd);
        }

        [TestMethod]
        public void MediationTreeRootTest()
        {
            tree = new MediationTree(testDomain, testProblem, pathEvent);
            Assert.AreEqual(tree.Root.Outgoing.Count, 4);
            Assert.AreEqual(tree.Root.Outgoing.FindAll(x => x.ActionType == ActionType.Constituent).Count, 1);
        }

        [TestMethod]
        public void MediationTreeUpdateTest()
        {
            tree = new MediationTree(testDomain, testProblem, pathEvent);
            MediationTreeNode child = tree.GetNode(tree.Root.Domain, tree.Root.Problem, tree.Root.Outgoing.Find(x => x.ActionType == ActionType.Constituent));

            Assert.AreEqual(child.Outgoing.Count, 2);
            Assert.AreEqual(child.Outgoing.Find(x => x.Action.Name.Equals("move-location")).ActionType, ActionType.Constituent);
            Assert.AreEqual(child.Outgoing.Find(x => x.Action.Name.Equals("toggle-green")).ActionType, ActionType.Consistent);

            child = tree.GetNode(child.Domain, child.Problem, child.Outgoing.Find(x => x.Action.Name.Equals("move-location")));
            Assert.AreEqual(child.Outgoing.FindAll(x => x.ActionType == ActionType.Constituent).Count, 1);
        }
    }
}
