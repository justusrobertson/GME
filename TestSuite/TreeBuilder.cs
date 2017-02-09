using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

using Mediation.FileIO;
using Mediation.PlanTools;
using Mediation.Planners;
using Mediation.StateSpace;
using Mediation.Enums;
using Mediation.Utilities;
using Mediation.GameTree;
using Mediation.MediationTree;

namespace TestSuite
{
    class TreeBuilder
    {
        public static void MediationTreeBuilder (string domainName, int expand)
        {
            // Parse the domain file.
            Domain domain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + domainName + @"\domain.pddl", PlanType.StateSpace);

            // Parse the problem file.
            Problem problem = Parser.GetProblemWithTypes(Parser.GetTopDirectory() + @"Benchmarks\" + domainName + @"\prob01.pddl", domain);

            // Create the initial node of mediation space.
            MediationTree tree = new MediationTree (domain, problem, Parser.GetTopDirectory() + @"MediationTrees\Data\" + domain.Name + @"\");

            // Remember the game tree path.
            string dataPath = Parser.GetTopDirectory() + @"TestLogs\" + domainName + @"\";

            // Check each path to see if it exists. If not, create the folder.
            if (!File.Exists(dataPath))
                Directory.CreateDirectory(dataPath);

            TestData data = new TestData();

            Stopwatch watch = new Stopwatch();

            // If data already exists, load it from memory.
            if (File.Exists(dataPath + "mediationtreedata")) data = BinarySerializer.DeSerializeObject<TestData>(dataPath + "mediationtreedata");
            else
            {
                data.elapsedMilliseconds = 0;
                data.frontier = new List<int>() { 0 };
                data.depth = 0;
                data.nodeCounter = 1;
                data.goalStateCount = 0;
                data.deadEndCount = 0;
                data.summaries = new List<List<Tuple<string, string>>>();
            }

            watch.Start();

            while (expand > 0 && data.frontier.Count > 0)
            {
                MediationTreeNode current = tree.GetNode(data.frontier[0]);
                foreach (MediationTreeEdge edge in current.Outgoing)
                {
                    MediationTreeNode child = tree.GetNode(current.Domain, current.Problem, edge);
                    if (child.Depth > data.depth) data.depth = child.Depth;
                    data.nodeCounter++;
                    if (child.IsGoal) data.goalStateCount++;
                    if (child.DeadEnd) data.deadEndCount++;
                    data.frontier.Add(child.ID);
                    expand--;
                }
                data.frontier.RemoveAt(0);
                data.elapsedMilliseconds += watch.ElapsedMilliseconds;
                watch.Reset();
                BinarySerializer.SerializeObject<TestData>(dataPath + "mediationtreedata", data);
            }

            watch.Stop();
            data.summaries.Add(WriteTree(domainName, data));
            BinarySerializer.SerializeObject<TestData>(dataPath + "mediationtreedata", data);
        }

        // Creates a single tree of specified depth without specifying a folder name.
        public static void MCTSearch (string domainName, int totalPlays, int interval)
        {
            // Save the summaries of each build.
            List<List<Tuple<String, String>>> summaries = new List<List<Tuple<String, String>>>();

            // Remember a time stamp for the top directory.
            string timeStamp = DateTime.Now.ToString("MM-dd-yyyy-HH-mm-tt");

            // Read in the domain file.
            Domain domain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + domainName + @"\domain.pddl", PlanType.StateSpace);

            // Read in the problem file.
            Problem problem = Parser.GetProblemWithTypes(Parser.GetTopDirectory() + @"Benchmarks\" + domainName + @"\prob01.pddl", domain);

            // Create a stopwatch object.
            Stopwatch watch = new Stopwatch();

            // Remember the game tree path.
            string path = Parser.GetTopDirectory() + @"GameTrees\Data\" + domainName + @"\";

            // Check each path to see if it exists. If not, create the folder.
            if (!File.Exists(path))
                Directory.CreateDirectory(path);

            // Start the stopwatch.
            watch.Start();

            // Initialize the game tree.
            GameTree tree = null;
            if (File.Exists(path + "gametree")) tree = BinarySerializer.DeSerializeObject<GameTree>(path + "gametree");
            else tree = new GameTree(domain, problem, path);

            // Stop the stopwatch.
            watch.Stop();

            // Loop through the depths.
            for (int plays = 0; plays < totalPlays; plays = plays + interval)
            {
                Console.Out.WriteLine("Starting play " + plays);

                // Print the current tree.
                summaries.Add(WriteTree(domainName, timeStamp, plays, watch, tree, false));

                // Start the watch.
                watch.Start();

                // Play the next round.
                MCTS.Search(interval, tree);

                // Save the game tree.
                BinarySerializer.SerializeObject<GameTree>(path + "gametree", tree);

                // Stop the watch.
                watch.Stop();
            }

            Console.Out.WriteLine("Printing tree.");

            // Print the current tree.
            summaries.Add(WriteTree(domainName, timeStamp, totalPlays, watch, tree, false));

            // Write the summary CSV file to disk.
            WriteGTSummary(domainName, timeStamp, summaries);

            // Use the CSV file to create an Excel spreadsheet and graphs of each summary element.
            Grapher.CreateGTGraphs(domainName, timeStamp, (totalPlays / interval) + 2, summaries);
        }

        // Creates a single tree of specified depth without specifying a folder name.
        public static void BreadthFirst (string domainName, int endDepth)
        {
            // Save the summaries of each build.
            List<List<Tuple<String, String>>> summaries = new List<List<Tuple<String, String>>>();

            // Remember a time stamp for the top directory.
            string timeStamp = DateTime.Now.ToString("MM-dd-yyyy-HH-mm-tt");

            // Read in the domain file.
            Domain domain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + domainName + @"\domain.pddl", PlanType.StateSpace);

            // Read in the problem file.
            Problem problem = Parser.GetProblem(Parser.GetTopDirectory() + @"Benchmarks\" + domainName + @"\prob01.pddl");

            // Create a stopwatch object.
            Stopwatch watch = new Stopwatch();

            // Start the stopwatch.
            watch.Start();

            // Find an initial plan.
            Plan plan = FastDownward.Plan(domain, problem);

            // Create the root node.
            StateSpaceNode root = StateSpaceSearchTools.CreateNode(Planner.FastDownward, domain, problem, plan, plan.Initial as State);

            // Stop the stopwatch.
            watch.Stop();

            // Create the frontier.
            List<StateSpaceNode> frontier = new List<StateSpaceNode>() { root };

            // Loop through the depths.
            for (int depth = 0; depth < endDepth; depth++)
            {
                // Print the current tree.
                summaries.Add(WriteTree(domainName, timeStamp, depth, watch, root));

                // Start the watch.
                watch.Start();

                // Create the new frontier.
                List<StateSpaceNode> newFrontier = StateSpaceMediator.BuildLayer(Planner.FastDownward, frontier);

                // Update the frontier.
                frontier = newFrontier;

                // Stop the watch.
                watch.Stop();
            }

            // Print the current tree.
            summaries.Add(WriteTree(domainName, timeStamp, endDepth, watch, root));

            // Write the summary CSV file to disk.
            WriteSummary(domainName, timeStamp, summaries);

            // Use the CSV file to create an Excel spreadsheet and graphs of each summary element.
            Grapher.CreateGraphs(domainName, timeStamp, endDepth + 2, summaries);
        }

        // Creates multiple tree depths, starting with zero.
        public static void MultipleTrees (string domainName, int endDepth)
        {
            // Call the multiple tree method with zero as the start parameter.
            MultipleTrees(domainName, 0, endDepth);
        }

        // Build a range of tree depths.
        public static void MultipleTrees (string domainName, int startDepth, int endDepth)
        {
            // Save the summaries of each build.
            List<List<Tuple<String, String>>> summaries = new List<List<Tuple<String, String>>>();

            // Remember a time stamp for the top directory.
            string timeStamp = DateTime.Now.ToString("MM-dd-yyyy-HH-mm-tt");

            // Loop through the depths.
            for (int depth = startDepth; depth <= endDepth; depth++)
            {
                // Build the tree and save its summary.
                summaries.Add(SingleTree(domainName, timeStamp, depth));

                // Clear the mediator's memory.
                StateSpaceMediator.Clear();
            }

            // Write the summary CSV file to disk.
            WriteSummary(domainName, timeStamp, summaries);

            // Use the CSV file to create an Excel spreadsheet and graphs of each summary element.
            Grapher.CreateGraphs(domainName, timeStamp, endDepth - startDepth + 2, summaries);
        }

        // Creates a single tree of specified depth without specifying a folder name.
        public static void SingleTree (string domainName, int depth)
        {
            // Create the top level folder name and create the tree.
            SingleTree(domainName, DateTime.Now.ToString("MM-dd-yyyy-HH-mm-tt"), depth);
        }

        // Creates a single tree of specified depth.
        private static List<Tuple<String, String>> SingleTree (string domainName, string timeStamp, int depth)
        {
            // Read in the domain file.
            Domain domain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + domainName + @"\domain.pddl", PlanType.StateSpace);

            // Read in the problem file.
            Problem problem = Parser.GetProblem(Parser.GetTopDirectory() + @"Benchmarks\" + domainName + @"\prob01.pddl");

            // Find an initial plan.
            Plan plan = FastDownward.Plan(domain, problem);

            // Create a stopwatch object.
            Stopwatch watch = new Stopwatch();

            // Start the stopwatch.
            watch.Start();

            // Use the state space mediator to build the tree.
            StateSpaceNode root = StateSpaceMediator.BuildTree(Planner.FastDownward, domain, problem, plan, plan.Initial as State, depth);

            // Stop the stopwatch.
            watch.Stop();

            // Write the tree to disk using HTML files and return the summary object.
            return WriteTree(domainName, timeStamp, depth, watch, root);
        }

        // Writes a mediation tree to disk with a summary file.
        private static List<Tuple<String, String>> WriteTree (string domainName, string timeStamp, int depth, Stopwatch watch, StateSpaceNode root)
        {
            // Create the path information.
            string outputDir = Parser.GetTopDirectory() + @"TestLogs\";
            string domainDir = outputDir + domainName + @"\";
            string timeDir = domainDir + timeStamp + @"\";
            string depthDir = timeDir + depth + @"\";

            // Check each path to see if it exists. If not, create the folder.
            if (!File.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            if (!File.Exists(domainDir))
                Directory.CreateDirectory(domainDir);

            if (!File.Exists(timeDir))
                Directory.CreateDirectory(timeDir);

            if (!File.Exists(depthDir))
                Directory.CreateDirectory(depthDir);

            // Create the summary elements and populate their values.
            List<Tuple<String, String>> summary = new List<Tuple<String, String>>();
            summary.Add(new Tuple<String, String>("Depth", depth.ToString()));
            summary.Add(new Tuple<String, String>("Node Count", StateSpaceMediator.NodeCount.ToString()));
            summary.Add(new Tuple<String, String>("Outgoing Edge Count", StateSpaceMediator.OutgoingEdgesCount.ToString()));
            summary.Add(new Tuple<String, String>("Average Branching Factor", ((float)StateSpaceMediator.OutgoingEdgesCount / (float)StateSpaceMediator.NodeCount).ToString()));
            summary.Add(new Tuple<String, String>("Goal State Count", StateSpaceMediator.GoalStateCount.ToString()));
            summary.Add(new Tuple<String, String>("Dead End Count", StateSpaceMediator.DeadEndCount.ToString()));
            if (StateSpaceMediator.DeadEndCount != 0) summary.Add(new Tuple<String, String>("Node to Dead End Ratio", ((float)StateSpaceMediator.NodeCount / (float)StateSpaceMediator.DeadEndCount).ToString()));
            else summary.Add(new Tuple<String, String>("Node to Dead End Ratio", StateSpaceMediator.NodeCount.ToString()));
            summary.Add(new Tuple<String, String>("Constituent Edge Count", StateSpaceMediator.Constituent.ToString()));
            summary.Add(new Tuple<String, String>("Consistent Edge Count", StateSpaceMediator.Consistent.ToString()));
            summary.Add(new Tuple<String, String>("Exceptional Edge Count", StateSpaceMediator.Exceptional.ToString()));
            summary.Add(new Tuple<String, String>("Elapsed Time in Milliseconds", watch.ElapsedMilliseconds.ToString()));
            summary.Add(new Tuple<String, String>("Elapsed Time in Seconds", (watch.ElapsedMilliseconds / 1000).ToString()));
            summary.Add(new Tuple<String, String>("Elapsed Time in Minutes", (watch.ElapsedMilliseconds / 60000).ToString()));

            // Create the summary.
            Writer.Summary(depthDir, summary);

            // Write the tree to disk.
            Writer.ToHTML(depthDir, root);

            // Return the summary.
            return summary;
        }

        // Writes a game tree to disk with a summary file.
        private static List<Tuple<String, String>> WriteTree (string domainName, string timeStamp, int round, Stopwatch watch, GameTree tree, bool printTree)
        {
            // Create the path information.
            string outputDir = Parser.GetTopDirectory() + @"GameTrees\";
            string domainDir = outputDir + domainName + @"\";
            string timeDir = domainDir + timeStamp + @"\";
            string depthDir = timeDir + round + @"\";

            // Check each path to see if it exists. If not, create the folder.
            if (!File.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            if (!File.Exists(domainDir))
                Directory.CreateDirectory(domainDir);

            if (!File.Exists(timeDir))
                Directory.CreateDirectory(timeDir);

            if (!File.Exists(depthDir))
                Directory.CreateDirectory(depthDir);

            // Create the summary elements and populate their values.
            List<Tuple<String, String>> summary = new List<Tuple<String, String>>();
            summary.Add(new Tuple<String, String>("Games Played", tree.TotalPlays.ToString()));
            summary.Add(new Tuple<String, String>("Total Wins", tree.TotalWins.ToString()));
            summary.Add(new Tuple<String, String>("Total Losses", tree.TotalLosses.ToString()));
            if (tree.TotalLosses != 0) summary.Add(new Tuple<String, String>("W-L Ratio", ((double)tree.TotalWins / (double)tree.TotalLosses).ToString()));
            else summary.Add(new Tuple<String, String>("W-L Ratio", "1"));
            summary.Add(new Tuple<String, String>("Node Count", tree.TotalNodes.ToString()));
            //summary.Add(new Tuple<String, String>("Outgoing Edge Count", root.EdgeCount.ToString()));
            summary.Add(new Tuple<String, String>("Lowest Depth Reached", tree.LowestDepth.ToString()));
            summary.Add(new Tuple<String, String>("Goal State Count", tree.GoalStateCount.ToString()));
            summary.Add(new Tuple<String, String>("Dead End Count", tree.DeadEndCount.ToString()));
            if (tree.DeadEndCount != 0) summary.Add(new Tuple<String, String>("Node to Dead End Ratio", ((float)tree.TotalNodes / (float)tree.DeadEndCount).ToString()));
            else summary.Add(new Tuple<String, String>("Node to Dead End Ratio", tree.TotalNodes.ToString()));
            summary.Add(new Tuple<String, String>("Elapsed Time in Milliseconds", watch.ElapsedMilliseconds.ToString()));
            summary.Add(new Tuple<String, String>("Elapsed Time in Seconds", (watch.ElapsedMilliseconds / 1000).ToString()));
            summary.Add(new Tuple<String, String>("Elapsed Time in Minutes", (watch.ElapsedMilliseconds / 60000).ToString()));

            // Create the summary.
            Writer.Summary(depthDir, summary);

            // Write the tree to disk.
            if (printTree) Writer.ToHTML(depthDir, tree);

            // Return the summary.
            return summary;
        }

        // Writes a game tree to disk with a summary file.
        private static List<Tuple<String, String>> WriteTree (string domainName, TestData data)
        {
            // Create the path information.
            string outputDir = Parser.GetTopDirectory() + @"TestLogs\";
            string domainDir = outputDir + domainName + @"\";

            // Check each path to see if it exists. If not, create the folder.
            if (!File.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            if (!File.Exists(domainDir))
                Directory.CreateDirectory(domainDir);

            // Create the summary elements and populate their values.
            List<Tuple<String, String>> summary = new List<Tuple<String, String>>();
            summary.Add(new Tuple<String, String>("Nodes Expanded", data.nodeCounter.ToString()));
            summary.Add(new Tuple<String, String>("Lowest Depth Reached", data.depth.ToString()));
            summary.Add(new Tuple<String, String>("Goal State Count", data.goalStateCount.ToString()));
            summary.Add(new Tuple<String, String>("Dead End Count", data.deadEndCount.ToString()));
            if (data.deadEndCount != 0) summary.Add(new Tuple<String, String>("Node to Dead End Ratio", ((float)data.nodeCounter / (float)data.deadEndCount).ToString()));
            else summary.Add(new Tuple<String, String>("Node to Dead End Ratio", data.nodeCounter.ToString()));
            summary.Add(new Tuple<String, String>("Elapsed Time in Milliseconds", data.elapsedMilliseconds.ToString()));
            summary.Add(new Tuple<String, String>("Elapsed Time in Seconds", (data.elapsedMilliseconds / 1000).ToString()));
            summary.Add(new Tuple<String, String>("Elapsed Time in Minutes", (data.elapsedMilliseconds / 60000).ToString()));

            // Return the summary.
            return summary;
        }

        // Creates a CSV file that summarizes the trees built during the session.
        private static void WriteSummary (string domainName, string timeStamp, List<List<Tuple<String, String>>> summaries)
        {
            // Create the path information.
            string outputDir = Parser.GetTopDirectory() + @"TestLogs\";
            string domainDir = outputDir + domainName + @"\";
            string timeDir = domainDir + timeStamp + @"\";

            // Check each path to see if it exists. If not, create the folder.
            if (!File.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            if (!File.Exists(domainDir))
                Directory.CreateDirectory(domainDir);

            if (!File.Exists(timeDir))
                Directory.CreateDirectory(timeDir);

            // Write the summaries out to a CSV file.
            Writer.ToCSV(timeDir, summaries);
        }

        // Creates a CSV file that summarizes the trees built during the session.
        private static void WriteGTSummary (string domainName, string timeStamp, List<List<Tuple<String, String>>> summaries)
        {
            // Create the path information.
            string outputDir = Parser.GetTopDirectory() + @"GameTrees\";
            string domainDir = outputDir + domainName + @"\";
            string timeDir = domainDir + timeStamp + @"\";

            // Check each path to see if it exists. If not, create the folder.
            if (!File.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            if (!File.Exists(domainDir))
                Directory.CreateDirectory(domainDir);

            if (!File.Exists(timeDir))
                Directory.CreateDirectory(timeDir);

            // Write the summaries out to a CSV file.
            Writer.ToCSV(timeDir, summaries);
        }

        static void Main (string[] args)
        {
            MediationTreeBuilder("spy-types", 10);
        }
    }
}
