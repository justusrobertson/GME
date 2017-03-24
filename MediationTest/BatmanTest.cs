using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mediation.PlanTools;
using Mediation.FileIO;
using Mediation.Planners;
using Mediation.Enums;
using Mediation.MediationTree;

namespace MediationTest
{
    /// <summary>
    /// Summary description for BatmanTest
    /// </summary>
    [TestClass]
    public class BatmanTest
    {
        string testDomainName;
        string testDomainDirectory;
        Domain testDomain;
        Problem testProblem;
        Plan testPlan;
        string path;
        MediationTree tree;

        public BatmanTest()
        {
            testDomainName = "batman";
            testDomainDirectory = Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl";
            testDomain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl", PlanType.StateSpace);
            testProblem = Parser.GetProblem(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\prob01.pddl");
            testPlan = FastDownward.Plan(testDomain, testProblem);
        }

        [TestMethod]
        public void BatmanTestMediationTree()
        {
            path = Parser.GetTopDirectory() + @"MediationTrees\Data\Unit-Tests\Batman\vanilla\";
            Assert.IsTrue(testPlan.Steps.Count > 0);
            tree = new MediationTree(testDomain, testProblem, path, false, false, false);
            Assert.IsTrue(tree.Root.Plan.Steps.Count > 0);
            Assert.IsTrue(tree.Root.Outgoing.Count == 1);
            MediationTreeNode current = tree.GetNode(tree.Root.Domain, tree.Root.Problem, tree.Root.Outgoing[0]);
            Assert.IsTrue(current.Outgoing.Count == 1);
            current = tree.GetNode(tree.Root.Domain, tree.Root.Problem, current.Outgoing[0]);
            Assert.IsTrue(current.Outgoing.Count == 1);
            current = tree.GetNode(tree.Root.Domain, tree.Root.Problem, current.Outgoing[0]);
            Assert.IsTrue(current.Outgoing.Count == 2);
            current = tree.GetNode(tree.Root.Domain, tree.Root.Problem, current.Outgoing[0]);
            Assert.IsTrue(current.Outgoing.Count == 2);
            current = tree.GetNode(tree.Root.Domain, tree.Root.Problem, current.Outgoing[0]);
            current = tree.GetNode(tree.Root.Domain, tree.Root.Problem, current.Outgoing[0]);
            current = tree.GetNode(tree.Root.Domain, tree.Root.Problem, current.Outgoing[0]);
            current = tree.GetNode(tree.Root.Domain, tree.Root.Problem, current.Outgoing[0]);
            current = tree.GetNode(tree.Root.Domain, tree.Root.Problem, current.Outgoing[0]);
            current = tree.GetNode(tree.Root.Domain, tree.Root.Problem, current.Outgoing[0]);
            current = tree.GetNode(tree.Root.Domain, tree.Root.Problem, current.Outgoing[0]);
            current = tree.GetNode(tree.Root.Domain, tree.Root.Problem, current.Outgoing[0]);
            current = tree.GetNode(tree.Root.Domain, tree.Root.Problem, current.Outgoing[0]);
            current = tree.GetNode(tree.Root.Domain, tree.Root.Problem, current.Outgoing[0]);
            current = tree.GetNode(tree.Root.Domain, tree.Root.Problem, current.Outgoing[0]);
            current = tree.GetNode(tree.Root.Domain, tree.Root.Problem, current.Outgoing[0]);
            current = tree.GetNode(tree.Root.Domain, tree.Root.Problem, current.Outgoing[0]);
            current = tree.GetNode(tree.Root.Domain, tree.Root.Problem, current.Outgoing[0]);
            Assert.IsTrue(current.IsGoal);
        }

        [TestMethod]
        public void BatmanTestEventRevision()
        {
            path = Parser.GetTopDirectory() + @"MediationTrees\Data\Unit-Tests\Batman\event\";
            Assert.IsTrue(testPlan.Steps.Count > 0);
            tree = new MediationTree(testDomain, testProblem, path, false, true, false);
            Assert.IsTrue(tree.Root.Plan.Steps.Count > 0);
            Assert.IsTrue(tree.Root.Outgoing.Count == 1);
            MediationTreeNode current = tree.GetNode(tree.Root.Domain, tree.Root.Problem, tree.Root.Outgoing[0]);
            Assert.IsTrue(current.Outgoing.Count == 1);
            current = tree.GetNode(tree.Root.Domain, tree.Root.Problem, current.Outgoing[0]);
            Assert.IsTrue(current.Outgoing.Count == 1);
            current = tree.GetNode(tree.Root.Domain, tree.Root.Problem, current.Outgoing[0]);
            Assert.IsTrue(current.Outgoing.Count == 2);
            current = tree.GetNode(tree.Root.Domain, tree.Root.Problem, current.Outgoing[0]);
            Assert.IsTrue(current.Outgoing.Count == 2);
            current = tree.GetNode(tree.Root.Domain, tree.Root.Problem, current.Outgoing[0]);
            current = tree.GetNode(tree.Root.Domain, tree.Root.Problem, current.Outgoing[0]);
            current = tree.GetNode(tree.Root.Domain, tree.Root.Problem, current.Outgoing[0]);
            current = tree.GetNode(tree.Root.Domain, tree.Root.Problem, current.Outgoing[0]);
            current = tree.GetNode(tree.Root.Domain, tree.Root.Problem, current.Outgoing[0]);
            current = tree.GetNode(tree.Root.Domain, tree.Root.Problem, current.Outgoing[0]);
            current = tree.GetNode(tree.Root.Domain, tree.Root.Problem, current.Outgoing[0]);
            current = tree.GetNode(tree.Root.Domain, tree.Root.Problem, current.Outgoing[1]);
            Assert.IsFalse(current.DeadEnd);
        }
    }
}
