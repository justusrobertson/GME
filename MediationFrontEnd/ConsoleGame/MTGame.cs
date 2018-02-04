using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;

using Mediation.StateSpace;
using Mediation.PlanTools;
using Mediation.Planners;
using Mediation.FileIO;
using Mediation.KnowledgeTools;
using Mediation.Enums;
using Mediation.Interfaces;
using Mediation.MediationTree;

using MediationFrontEnd.Discourse;

namespace MediationFrontEnd.ConsoleGame
{
    static class MTGame
    {
        // The mediation tree.
        public static MediationTree tree;

        public static MediationTreeNode current;

        public static List<IOperator> computerActions;

        // The raw input.
        public static string input = "";

        // The input command.
        public static string command = "";

        // The command's arguments.
        public static List<string> arguments;

        // The mediation frontier.
        public static Hashtable frontier;

        // The frontier thread.
        public static Thread frontierThread;

        // The debug switch.
        public static bool debug = false;

        public static bool twoTurns = false;

        public static bool exploreFrontier = true;

        // The current planner.
        public static Planner planner = Planner.FastDownward;

        // Error responses.
        public static string[] responses =
        {
            "I'm afraid you can't do that.",
            "You can't get ye flask!"
        };

        /// <summary>
        /// An online text-based command line game that is a traversal of mediation space.
        /// </summary>
        /// <param name="domainName">The game domain.</param>
        /// <param name="debug">Whether or not debug mode is enabled.</param>
        public static void Play()
        {
            Console.Clear();
            string domainName = "";

            // Make sure the file exists.
            while
                (!File.Exists(Parser.GetTopDirectory() + @"Benchmarks\" + domainName + @"\domain.pddl") ||
                 !File.Exists(Parser.GetTopDirectory() + @"Benchmarks\" + domainName + @"\prob01.pddl"))
            {
                // Prompt the user for game name.
                Console.WriteLine("What would you like to play?");
                Console.WriteLine("     Type 'exit' to end program.");
                Console.WriteLine("     Type 'path' to modify the directory.");
                Console.WriteLine("     Type 'planner' to choose the planner.");
                Console.WriteLine("     Type 'help' to print game titles.");
                Console.Write("     Type 'debug' to turn debug mode ");
                if (debug) Console.WriteLine("off.");
                else Console.WriteLine("on.");
                Console.Write("     Type 'turns' to switch turn-taking ");
                if (twoTurns) Console.WriteLine("off.");
                else Console.WriteLine("on.");
                Console.WriteLine();
                Console.Write(">");

                // Read in the game to load.
                domainName = Console.ReadLine().ToLower();

                // Print domains if prompted.
                if (domainName.Equals("help"))
                {
                    Console.WriteLine();
                    if (System.IO.Directory.Exists(Parser.GetTopDirectory() + @"Benchmarks\"))
                    {
                        foreach (string file in Directory.GetFileSystemEntries(Parser.GetTopDirectory() + @"Benchmarks\"))
                            Console.WriteLine(Path.GetFileName(file));

                        Console.WriteLine();
                        Console.Write(">");

                        // Read in the game to load.
                        domainName = Console.ReadLine().ToLower();
                    }
                    else
                    {
                        Console.WriteLine(Parser.GetTopDirectory() + @"Benchmarks\ does not exist!");
                        Console.ReadKey();
                        domainName = "482990adkdlllifkdlkfjlaoow";
                    }
                }

                // Rewrite directory if prompted.
                if (domainName.Equals("path"))
                {
                    Console.WriteLine();
                    Console.WriteLine("Your current game directory is: " + Parser.GetTopDirectory());
                    Console.WriteLine("Enter new path.");
                    Console.WriteLine();
                    Console.Write(">");
                    string newPath = Console.ReadLine();
                    Console.WriteLine();

                    if (Directory.Exists(newPath + @"Benchmarks\")) Parser.path = newPath;
                    else
                    {
                        Console.WriteLine("Sorry, " + newPath + @"Benchmarks\" + " does not exist.");
                        Console.ReadKey();
                    }
                }

                // Change the planner if prompted.
                if (domainName.Equals("planner"))
                {
                    Console.WriteLine();
                    Console.WriteLine("Your current planner is: " + planner);
                    Console.WriteLine("Enter new planner.");
                    Console.WriteLine();
                    Console.Write(">");
                    string newPlanner = Console.ReadLine();
                    Console.WriteLine();

                    if (Enum.IsDefined(typeof(Planner), newPlanner))
                    {
                        planner = (Planner)Enum.Parse(typeof(Planner), newPlanner, true);
                        Console.WriteLine("Your planner is now " + planner);
                        Console.ReadKey();
                    }
                    else
                    {
                        Console.WriteLine("Sorry, " + newPlanner + " does not exist.");
                        Console.ReadKey();
                    }
                }

                // Exit if prompted.
                if (domainName.Equals("exit")) Environment.Exit(0);

                // Toggle debug if prompted
                if (domainName.Equals("debug"))
                    debug = !debug;

                if (domainName.Equals("turns"))
                    twoTurns = !twoTurns;

                if
                    ((!File.Exists(Parser.GetTopDirectory() + @"Benchmarks\" + domainName + @"\domain.pddl") ||
                     !File.Exists(Parser.GetTopDirectory() + @"Benchmarks\" + domainName + @"\prob01.pddl")) &&
                     !domainName.Equals("debug") && !domainName.Equals("path") && !domainName.Equals("planner") 
                     && !domainName.Equals("482990adkdlllifkdlkfjlaoow") && !domainName.Equals("turns"))
                {
                    // Prompt that the game doesn't exist.
                    Console.WriteLine();
                    Console.WriteLine("I'm sorry, but I can't find " + domainName.ToUpper());
                    Console.WriteLine();
                    Console.ReadKey();
                }

                // Clear the console screen.
                Console.Clear();
            }

            // Parse the domain file.
            Domain domain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + domainName + @"\domain.pddl", PlanType.StateSpace);

            // Parse the problem file.
            Problem problem = Parser.GetProblemWithTypes(Parser.GetTopDirectory() + @"Benchmarks\" + domainName + @"\prob01.pddl", domain);

            // Welcome the player to the game.
            Console.WriteLine("Welcome to " + domain.Name);

            // Create the initial node of mediation space.
            tree = new MediationTree (domain, problem, Parser.GetTopDirectory() + @"MediationTrees\Data\" + domain.Name + @"\", false, false, false);

            if (debug)
            {
                Console.WriteLine();
                Console.WriteLine("Nodes Expanded: " + tree.TotalNodes);
                Console.WriteLine("Dead Ends Expanded: " + tree.DeadEndCount);
                Console.WriteLine("Goal States Expanded: " + tree.GoalStateCount);
                Console.WriteLine("Lowest Depth Expanded: " + tree.LowestDepth);
            }

            current = tree.Root;

            computerActions = new List<IOperator>();

            if (!twoTurns)
                while (!tree.GetTurnAtIndex(current.Depth).Equals(current.Problem.Player)) TakeTurn();

            // Initialize a stopwatch for debugging.
            Stopwatch watch = new Stopwatch();

            Console.Out.WriteLine();

            // Present the initial state.
            command = "";
            Look(false);

            // Loop while this is false.
            bool exit = false;
            while (!exit)
            {
                exploreFrontier = true;

                // Initialize the frontier.
                frontier = new Hashtable();

                // Expand the frontier in a new thread.
                frontierThread = new Thread(ExpandFrontier);

                // Start the thread.
                frontierThread.Start();

                // Ask for input.
                Console.WriteLine();
                Console.Write(">");
                input = Console.ReadLine();

                // If in debug mode, start the stop watch.
                if (debug)
                {
                    watch.Reset();
                    watch.Start();
                }

                // Parse the command and its arguments.
                command = ParseCommand(input).ToLower();
                arguments = ParseArguments(input);

                // Interpret the command.
                switch (command)
                {
                    case "exit":
                        exit = true;
                        break;
                    case "clear":
                        Console.Clear();
                        break;
                    case "cls":
                        Console.Clear();
                        break;
                    case "look":
                        Look(true);
                        break;
                    case "help":
                        Help();
                        break;
                    case "wait":
                        Console.Clear();
                        Wait();
                        break;
                    default:
                        OneArg();
                        break;
                }

                // If debugging, write the current plan and the elapsed time.
                if (debug && current.Plan.Steps.Count > 0 && !command.Equals("clear") && !command.Equals("cls"))
                {
                    Console.Out.WriteLine();
                    Console.WriteLine("Narrative Trajectory:");
                    int longestName = 0;
                    foreach (Operator step in current.Plan.Steps)
                        if (step.TermAt(0).Length > longestName)
                            longestName = step.TermAt(0).Length;
                    string lastName = "";
                    foreach (Operator step in current.Plan.Steps)
                    {
                        if (!step.TermAt(0).Equals(lastName))
                        {
                            lastName = step.TermAt(0);
                            Console.Out.Write("".PadLeft(5) + UppercaseFirst(step.TermAt(0)) + "".PadLeft(longestName - step.TermAt(0).Length + 1));
                        }
                        else
                            Console.Out.Write("".PadLeft(5) + "".PadLeft(longestName + 1));

                        string[] splitName = step.Name.Split('-');
                        Console.Out.Write(splitName[0] + "s ");
                        for (int i = 1; i < step.Name.Count(x => x == '-') + 1; i++)
                            Console.Out.Write(UppercaseFirst(step.TermAt(i)) + " ");
                        Console.Out.WriteLine();
                    }
                    Console.Out.WriteLine();
                    Console.Write("Elapsed time: ");
                    Console.Out.Write(watch.ElapsedMilliseconds);
                    Console.WriteLine("ms");
                }

                // Check for goal state.
                if (current.IsGoal)
                {
                    Console.WriteLine("GOAL STATE");
                    Console.ReadKey();
                    exit = true;
                }
                // Check for incompatible state.
                else if (current.DeadEnd)
                {
                    Console.WriteLine("UNWINNABLE STATE");
                    Console.ReadKey();
                    exit = true;
                }

                if (exit)
                    Console.Clear();

                // Kill the frontier thread (this should be okay).
                exploreFrontier = false;
            }

            MTGame.Play();
        }

        /// <summary>
        /// Provides a description of what the player currently sees.
        /// </summary>
        public static void Look(Boolean wipeScreen)
        {
            if (current.DeadEnd) return;

            if (!twoTurns)
                while (!tree.GetTurnAtIndex(current.Depth).Equals(current.Problem.Player)) TakeTurn();

            // Initialize the output string to a new line.
            String actionDescription = "";
            String stateDescription = Environment.NewLine;

            // Create a list of predicates to represent the current state.
            List<IPredicate> state = new List<IPredicate>();

            // Set the state predicate list to be the initial state of the planning problem.
            foreach (IPredicate pred in current.Problem.Initial)
                state.Add((Predicate)pred.Clone());

            // Create a list of objects to represent the problem's objects.
            List<IObject> probObs = new List<IObject>();

            // Clone the object list.
            foreach (IObject obj in current.Problem.Objects)
                probObs.Add((Obj)obj.Clone());

            if (command.Equals("look"))
                actionDescription += Environment.NewLine;

            state = KnowledgeAnnotator.Annotate(state, tree.GetTurnAtIndex(current.Depth));

            // If the player did not ask to look at their surroundings...
            if (!command.Equals("look"))
            {
                actionDescription += "Your name is " + tree.GetTurnAtIndex(current.Depth) + "." + Environment.NewLine;

                // Actions that took place.
                List<IOperator> actions = computerActions;
                if (current.Incoming != null)
                    actions.Insert(0, current.Incoming.Action);
                while (actions.Count > tree.TurnOrder.Count) actions.RemoveAt(actions.Count - 1);

                int obsCount = 0;

                // Loop through the system actions...
                foreach (IOperator action in actions)
                {
                    // If the player can see the action taking place...
                    if (KnowledgeAnnotator.GetLocation(action.Actor, current.Problem.Initial).Equals(KnowledgeAnnotator.GetLocation(tree.GetTurnAtIndex(current.Depth), current.Problem.Initial)))
                    {
                        if (obsCount++ > 0) Console.Out.WriteLine();

                        // Add the action to the output.
                        if (action.TermAt(0).Equals(tree.GetTurnAtIndex(current.Depth)))
                        {
                            actionDescription += "You ";
                            string[] splitName = action.Name.Split('-');
                            actionDescription += splitName[0] + " ";
                            for (int i = 1; i < action.Name.Count(x => x == '-') + 1; i++)
                                actionDescription += UppercaseFirst(action.TermAt(i)) + " ";
                            actionDescription += Environment.NewLine;
                        }
                        else
                        {
                            actionDescription += "You see " + action.TermAt(0) + " ";
                            string[] splitName = action.Name.Split('-');
                            actionDescription += splitName[0] + " ";
                            for (int i = 1; i < action.Name.Count(x => x == '-') + 1; i++)
                                actionDescription += UppercaseFirst(action.TermAt(i)) + " ";
                            actionDescription += Environment.NewLine;
                        }

                        // Add each action effect to the output.
                        foreach (Predicate effect in action.Effects)
                            if (effect.Name.Equals("at"))
                            {
                                if (effect.Sign)
                                    actionDescription += "".PadLeft(5) + UppercaseFirst(effect.TermAt(0).Constant) + " is at the " + UppercaseFirst(effect.TermAt(1).Constant);
                                else
                                    actionDescription += "".PadLeft(5) + UppercaseFirst(effect.TermAt(0).Constant) + " is not at the " + UppercaseFirst(effect.TermAt(1).Constant);
                            }
                            else
                                actionDescription += "".PadLeft(5) + effect.ToString().Replace("(", "").Replace(")", "");

                        foreach (Axiom conditional in action.Conditionals)
                            foreach (Predicate effect in conditional.Effects)
                                actionDescription += "".PadLeft(5) + effect.ToString().Replace("(", "").Replace(")", "");

                        // Add newlines.
                        actionDescription += Environment.NewLine;
                    }
                }

                // Add a description of the current location to the output.
                stateDescription += DiscourseGenerator.RoomDescription(tree.GetTurnAtIndex(current.Depth), state, probObs);
            }
            // If the player supplies no arguments...
            else if (command.Equals("look") && arguments.Count == 0)
                // Provide a description of the current location.
                stateDescription += DiscourseGenerator.RoomDescription(tree.GetTurnAtIndex(current.Depth), state, probObs);
            // If the player looks at themself...
            else if (command.Equals("look") && (arguments[0].Equals("me") || arguments[0].Equals(current.Problem.Player)))
                // Provide a description of the player's character.
                stateDescription += DiscourseGenerator.PlayerDescription(tree.GetTurnAtIndex(current.Depth), state);
            // If the player looks at the room...
            else if (command.Equals("look") && (arguments[0].Equals("here") || arguments[0].Equals(KnowledgeAnnotator.GetLocation(current.Problem.Player, current.Problem.Initial))))
                // Provide a description of the current location.
                stateDescription += DiscourseGenerator.RoomDescription(tree.GetTurnAtIndex(current.Depth), state, probObs);
            else
                // The player can't see it.
                stateDescription += "You can't see that.";

            // Clear the console.
            if (wipeScreen) Console.Clear();

            Console.Out.WriteLine(actionDescription);

            // Format the output string.
            if (!stateDescription.Equals(""))
                foreach (string s in Wrap(stateDescription, 79))
                    Console.Out.WriteLine(s);
        }

        /// <summary>
        /// Provides a list of possible user actions.
        /// </summary>
        public static void Help()
        {
            Console.Out.WriteLine();
            Console.Out.WriteLine("You can: ");
            foreach (MediationTreeEdge edge in current.Outgoing)
            {
                string[] splitName = edge.Action.Name.Split('-');
                Console.Out.Write(splitName[0] + " ");
                for (int i = 1; i < edge.Action.Name.Count(x => x == '-') + 1; i++)
                    Console.Out.Write(edge.Action.TermAt(i) + " ");
                Console.Out.WriteLine();
            }
        }

        /// <summary>
        /// Moves along the outgoing wait edge from the current node in mediation space.
        /// </summary>
        public static void Wait()
        {
            // Provide feedback to the player.
            Console.Out.WriteLine();
            Console.Out.WriteLine("Time passes...");

            // Loop through the outgoing edges...
            foreach (MediationTreeEdge edge in current.Outgoing)
                // Identify 
                if (edge.Action.Name.Equals("do-nothing"))
                    if (frontier.ContainsKey(edge)) current = frontier[edge] as MediationTreeNode;
                    else current = tree.GetNode(current.Domain, current.Problem, edge);

            Look(true);
        }

        public static void OneArg()
        {
            if (arguments.Count == 2)
            {
                TwoArgs();
                return;
            }
            else if (arguments.Count != 1)
            {
                Unknown();
                return;
            }

            MediationTreeEdge matchingEdge = null;
            foreach (MediationTreeEdge edge in current.Outgoing)
                if (edge.Action.Name.Count(x => x == '-') < 2)
                    if (edge.Action.Name.Contains('-'))
                    {
                        string[] split = edge.Action.Name.Split('-');
                        if (command.Equals(split[0]))
                            if (arguments[0].ToLower().Equals(edge.Action.TermAt(1)))
                                matchingEdge = edge;
                    }
                    else if (command.Equals(edge.Action.Name))
                        if (arguments[0].ToLower().Equals(edge.Action.TermAt(1)))
                            matchingEdge = edge;

            if (matchingEdge != null)
            {
                if (frontier.ContainsKey(matchingEdge))
                {
                    current = frontier[matchingEdge] as MediationTreeNode;
                }
                else
                {
                    exploreFrontier = false;
                    while (frontierThread.IsAlive) Thread.Sleep(50);
                    current = tree.GetNode(current.Domain, current.Problem, matchingEdge);
                }

                Look(true);
            }
            else
            {
                Console.Out.WriteLine();
                Random r = new Random();
                Console.Out.WriteLine(responses[r.Next(0, responses.Length)]);
                Console.Out.WriteLine("Try typing 'help'.");
            }
        }

        public static void TwoArgs()
        {
            if (arguments.Count != 2)
            {
                Unknown();
                return;
            }

            MediationTreeEdge matchingEdge = null;
            foreach (MediationTreeEdge edge in current.Outgoing)
                if (edge.Action.Name.Count(x => x == '-') == 2)
                    if (edge.Action.Name.Contains('-'))
                    {
                        string[] split = edge.Action.Name.Split('-');
                        if (command.Equals(split[0]))
                            if (arguments[0].ToLower().Equals(edge.Action.TermAt(1)))
                                if (arguments[1].ToLower().Equals(edge.Action.TermAt(2)))
                                    matchingEdge = edge;
                    }
                    else if (command.Equals(edge.Action.Name))
                        if (arguments[0].ToLower().Equals(edge.Action.TermAt(1)))
                            if (arguments[1].ToLower().Equals(edge.Action.TermAt(2)))
                                matchingEdge = edge;

            if (matchingEdge != null)
            {
                if (frontier.ContainsKey(matchingEdge)) current = frontier[matchingEdge] as MediationTreeNode;
                else
                {
                    exploreFrontier = false;
                    while (frontierThread.IsAlive) Thread.Sleep(50);
                    current = tree.GetNode(current.Domain, current.Problem, matchingEdge);
                }

                Look(true);
            }
            else
            {
                Console.Out.WriteLine();
                Random r = new Random();
                Console.Out.WriteLine(responses[r.Next(0, responses.Length)]);
                Console.Out.WriteLine("Try typing 'help'.");
            }
        }

        public static void Unknown()
        {
            MediationTreeEdge matchingEdge = null;
            foreach (MediationTreeEdge edge in current.Outgoing)
                if (command.Equals(edge.Action.Name) && arguments.Count == edge.Action.Arity)
                {
                    bool match = true;
                    for (int i = 0; i < arguments.Count; i++)
                        if (!arguments[i].ToLower().Equals(edge.Action.Predicate.TermAt(i)))
                            if (!arguments[i].ToLower().Equals(edge.Action.TermAt(i)))
                                match = false;

                    if (match)
                        matchingEdge = edge;
                }

            if (matchingEdge != null)
            {
                current = tree.GetNode(current.Domain, current.Problem, matchingEdge);
                Look(true);
            }
            else
            {
                exploreFrontier = false;
                while (frontierThread.IsAlive) Thread.Sleep(50);
                Console.Out.WriteLine();
                Random r = new Random();
                Console.Out.WriteLine(responses[r.Next(0, responses.Length)]);
                Console.Out.WriteLine("Try typing 'help'.");
            }
        }

        public static string ParseCommand(string line)
        {
            List<string> segments = line.Split(' ').ToList();
            return segments[0];
        }

        public static List<string> ParseArguments(string line)
        {
            List<string> segments = line.Split(' ').ToList();
            segments.RemoveAt(0);
            List<string> newSegs = new List<string>();
            foreach (string seg in segments)
                if (!IsPreposition(seg))
                    newSegs.Add(seg);
            return newSegs;
        }

        private static bool IsPreposition(string word)
        {
            List<string> prepositions = new List<string>
            {
                "from",
                "to",
                "the",
                "a",
                "an",
                "in"
            };

            string match = prepositions.FirstOrDefault(s => s.Contains(word));
            return match != null;
        }

        static List<string> Wrap(string text, int margin)
        {
            int start = 0, end;
            var lines = new List<string>();
            text = Regex.Replace(text, @"\s", " ").Trim();

            while ((end = start + margin) < text.Length)
            {
                while (text[end] != ' ' && end > start)
                    end -= 1;

                if (end == start)
                    end = start + margin;

                lines.Add(text.Substring(start, end - start));
                start = end + 1;
            }

            if (start < text.Length)
                lines.Add(text.Substring(start));

            return lines;
        }

        static void TakeTurn()
        {
            MediationTreeEdge matchingEdge = current.Outgoing.Find(x => x.ActionType.Equals(ActionType.Constituent));
            if (matchingEdge != null)
            {
                current = tree.GetNode(current.Domain, current.Problem, matchingEdge);
                computerActions.Add(matchingEdge.Action);
            }
        }

        static void ExpandFrontier()
        {
            foreach (MediationTreeEdge edge in current.Outgoing)
                if (exploreFrontier)
                {
                    MediationTreeNode newNode = tree.GetNode(current.Domain, current.Problem, edge);
                    frontier[edge] = newNode;
                }
        }

        static string UppercaseFirst(string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }
    }
}
