using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mediation.PlanTools;
using Mediation.FileIO;
using Mediation.Enums;
using Mediation.GameTree;

namespace MediationTest
{
    [TestClass]
    public class MCTSTest
    {
        string testDomainName;
        string testDomainDirectory;
        Domain testDomain;
        Problem testProblem;
        GameTree tree;
        string path;

        public MCTSTest()
        {
            testDomainName = "spy-types";
            testDomainDirectory = Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl";
            testDomain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl", PlanType.StateSpace);
            testProblem = Parser.GetProblemWithTypes(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\prob01.pddl", testDomain);
            path = Parser.GetTopDirectory() + @"GameTrees\Data\Unit-Tests\";
            tree = new GameTree(testDomain, testProblem, path);
        }

        [TestMethod]
        public void MCTSSearchTest()
        {
            MCTS.Search(10, tree);
            BinarySerializer.SerializeObject<GameTree>(Parser.GetTopDirectory() + @"GameTrees\Data\Unit-Tests\gametree", tree);
            Assert.AreEqual(tree.Root.Unplayed.Count, 0);
        }
    }
}
