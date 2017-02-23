using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mediation.PlanTools;
using Mediation.MediationTree;
using Mediation.FileIO;
using Mediation.Enums;
using System.Collections.Generic;
using Mediation.Interfaces;
using Mediation.Planners;

namespace MediationTest
{
    [TestClass]
    public class EventRevisorTest
    {
        string testDomainName;
        string testDomainDirectory;
        Domain testDomain;
        Problem testProblem;
        MediationTree tree;
        string path;

        public EventRevisorTest()
        {
            testDomainName = "spy-types";
            testDomainDirectory = Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl";
            testDomain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl", PlanType.StateSpace);
            testProblem = Parser.GetProblemWithTypes(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\prob01.pddl", testDomain);
            path = Parser.GetTopDirectory() + @"MediationTrees\Data\Unit-Tests\event\";
        }

        [TestMethod]
        public void EventRevisorHistoryTest()
        {
            MediationTreeNode child = BuildTree();
            List<MediationTreeNode> history = EventRevisor.GetWorldHistory(child, tree);
            Assert.AreEqual(history[0].ID, 0);
            Assert.AreEqual(history.Count, 7);
        }

        [TestMethod]
        public void EventRevisorKnowledgeTest()
        {
            MediationTreeNode child = BuildTree();
            List<Mediation.Utilities.Tuple<Operator, State>> history = EventRevisor.GetWorldKnowledge("snake", child, tree);
            Assert.AreEqual(history[0].First, null);
            Assert.AreEqual(history[0].Second.Predicates.FindAll(x => x.Name.Equals("at") && x.TermAt(0).Equals(new Term("boss", true))).Count, 1);
            Assert.AreEqual(history[0].Second.Predicates.FindAll(x => x.Name.Equals("at") && x.TermAt(0).Equals(new Term("snake", true))).Count, 5);
            Assert.AreEqual(history.Count, 7);
        }

        [TestMethod]
        public void EventRevisorObservedActionTest()
        {
            MediationTreeNode child = BuildTree();
            List<MediationTreeNode> history = EventRevisor.GetWorldHistory(child, tree);
            List<Mediation.Utilities.Tuple<Operator, State>> knowledge = new List<Mediation.Utilities.Tuple<Operator, State>>();
            foreach (MediationTreeNode node in history)
                if (node.Incoming != null) knowledge.Add(new Mediation.Utilities.Tuple<Operator, State>(node.Incoming.Action as Operator, new State()));
            Operator template = EventRevisor.GetObservedActionTemplate(1, knowledge[1].First);
            Assert.AreEqual(template.Name, "move-location*snake*gear*elevator");

            Problem newProb = testProblem.Clone() as Problem;
            Domain newDom = testDomain.Clone() as Domain;

            newProb.Name = "rob";
            newProb.Initial.Add(new Predicate("state-depth", new List<ITerm> { new Term("depth0", true) }, true));
            newProb.Objects.AddRange(new List<IObject> { new Obj("depth0", "integer"), new Obj("depth1", "integer"), new Obj("depth2", "integer"), new Obj("depth3", "integer"), new Obj("depth4", "integer"), new Obj("depth5", "integer") });
            newDom.AddTypePair("integer", "number");
            newDom.Predicates.Add(new Predicate("state-depth", new List<ITerm> { new Term("?integer", "", "number") }, true));

            newDom.Operators = new List<IOperator>();
            for (int i = 0; i < knowledge.Count - 1; i++)
                newDom.Operators.Add(EventRevisor.GetObservedActionTemplate(i, knowledge[i].First));

            foreach (Operator op in testDomain.Operators)
            {
                Operator newOp = op.Clone() as Operator;
                newOp.Preconditions.Add(new Predicate("state-depth", new List<ITerm> { new Term("depth5", true) }, true));
                newDom.Operators.Add(newOp);
            }

            Plan newPlan = FastDownward.Plan(newDom, newProb);
            Assert.AreNotEqual(newPlan.Steps.Count, 0);
        }

        [TestMethod]
        public void EventRevisorProblemPairTest()
        {
            MediationTreeNode child = BuildTree();
            Mediation.Utilities.Tuple<Domain, Problem> pair = EventRevisor.GetEventRevisionPair(child, tree);
            Plan newPlan = FastDownward.Plan(pair.First, pair.Second);
            Assert.AreNotEqual(newPlan.Steps.Count, 0);
        }

        private MediationTreeNode BuildTree ()
        {
            tree = new MediationTree(testDomain, testProblem, path, false, true);
            MediationTreeNode child = tree.GetNode(tree.Root.Domain, tree.Root.Problem, tree.Root.Outgoing.Find(x => x.Action.Name.Equals("move-location") && x.Action.TermAt(1).Equals("right")));

            child = tree.GetNode(child.Domain, child.Problem, child.Outgoing.Find(x => x.Action.Name.Equals("move-location")));
            child = tree.GetNode(child.Domain, child.Problem, child.Outgoing.Find(x => x.Action.Name.Equals("make-trap-wire")));

            child = tree.GetNode(child.Domain, child.Problem, child.Outgoing.Find(x => x.Action.Name.Equals("place-explosive-thing") && x.Action.TermAt(2).Equals("gears")));
            child = tree.GetNode(child.Domain, child.Problem, child.Outgoing.Find(x => x.Action.Name.Equals("set-trap")));

            child = tree.GetNode(child.Domain, child.Problem, child.Outgoing.Find(x => x.Action.Name.Equals("move-location") && x.Action.TermAt(1).Equals("left")));

            return child;
        }
    }
}
