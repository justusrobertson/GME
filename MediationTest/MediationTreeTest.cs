using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mediation.PlanTools;
using Mediation.MediationTree;
using Mediation.FileIO;
using Mediation.Enums;

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
        string path;

        public MediationTreeTest()
        {
            testDomainName = "spy-types";
            testDomainDirectory = Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl";
            testDomain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl", PlanType.StateSpace);
            testProblem = Parser.GetProblemWithTypes(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\prob01.pddl", testDomain);
            path = Parser.GetTopDirectory() + @"MediationTrees\Data\Unit-Tests\";
        }

        [TestMethod]
        public void EventRevisionTest()
        {
            tree = new MediationTree(testDomain, testProblem, path);
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
            tree = new MediationTree(testDomain, testProblem, path);
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
            child = tree.GetNode(child.Domain, child.Problem, child.Outgoing.Find(x => x.Action.Name.Equals("do nothing")));
            child = tree.GetNode(child.Domain, child.Problem, child.Outgoing.Find(x => x.Action.Name.Equals("detonate-explosive")));
            Assert.IsTrue(child.DeadEnd);
        }

        [TestMethod]
        public void MediationTreeRootTest()
        {
            tree = new MediationTree(testDomain, testProblem, path);
            Assert.AreEqual(tree.Root.Outgoing.Count, 4);
            Assert.AreEqual(tree.Root.Outgoing.FindAll(x => x.ActionType == ActionType.Constituent).Count, 1);
        }

        [TestMethod]
        public void MediationTreeUpdateTest()
        {
            tree = new MediationTree(testDomain, testProblem, path);
            MediationTreeNode child = tree.GetNode(tree.Root.Domain, tree.Root.Problem, tree.Root.Outgoing.Find(x => x.ActionType == ActionType.Constituent));

            Assert.AreEqual(child.Outgoing.Count, 2);
            Assert.AreEqual(child.Outgoing.Find(x => x.Action.Name.Equals("move-location")).ActionType, ActionType.Constituent);
            Assert.AreEqual(child.Outgoing.Find(x => x.Action.Name.Equals("toggle-green")).ActionType, ActionType.Consistent);

            child = tree.GetNode(child.Domain, child.Problem, child.Outgoing.Find(x => x.Action.Name.Equals("move-location")));
            Assert.AreEqual(child.Outgoing.FindAll(x => x.ActionType == ActionType.Constituent).Count, 1);
        }
    }
}
