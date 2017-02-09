using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mediation.PlanTools;
using Mediation.FileIO;
using Mediation.Enums;
using Mediation.GameTree;


namespace MediationTest
{
    [TestClass]
    public class GameTreeTest
    {
        string testDomainName;
        string testDomainDirectory;
        Domain testDomain;
        Problem testProblem;
        GameTree tree;

        public GameTreeTest()
        {
            testDomainName = "spy-types";
            testDomainDirectory = Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl";
            testDomain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl", PlanType.StateSpace);
            testProblem = Parser.GetProblemWithTypes(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\prob01.pddl", testDomain);
        }

        [TestMethod]
        public void GameTreeRootTest()
        {
            tree = new GameTree(testDomain, testProblem, Parser.GetTopDirectory() + @"GameTrees\Data\");
            GameTreeNode root = tree.Root;
            Assert.AreEqual(root.ID, 0);
            Assert.IsFalse(root.IsGoal);
            Assert.AreEqual(root.Outgoing.Count, 4);
            Assert.IsTrue(tree.Simulate(root.ID));
        }
    }
}
