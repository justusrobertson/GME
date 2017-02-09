using Mediation.PlanTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mediation.Planners;
using Mediation.Enums;

namespace Mediation.MediationTree
{
    /// <summary>
    /// Records statistics and performs operations for maintaining game trees.
    /// </summary>
    [Serializable]
    public class MediationTree
    {
        private Planner planner;

        /// <summary>
        /// Stores the current highest node ID.
        /// </summary>
        private int nodeCounter;

        /// <summary>
        /// Stores the lowest depth at which a node has been expanded.
        /// </summary>
        private int lowestDepth;
        public int LowestDepth
        {
            get { return lowestDepth; }
        }

        /// <summary>
        /// Stores how many goal states have been reached by the tree.
        /// </summary>
        private int goalStateCount;
        public int GoalStateCount
        {
            get { return goalStateCount; }
        }

        /// <summary>
        /// Stores how many dead ends have been reached by the tree.
        /// </summary>
        private int deadEndCount;
        public int DeadEndCount
        {
            get { return deadEndCount; }
        }

        /// <summary>
        /// Stores the total number of expanded nodes in the tree.
        /// </summary>
        public int TotalNodes
        {
            get { return nodeCounter + 1; }
        }

        /// <summary>
        /// The game tree's base domain object.
        /// </summary>
        private Domain domain;

        /// <summary>
        /// The game tree's base problem object.
        /// </summary>
        private Problem problem;

        /// <summary>
        /// The player's character.
        /// </summary>
        private string player;
        public string Player
        {
            get { return player; }
        }

        /// <summary>
        /// A list of the computer controlled characters.
        /// </summary>
        private List<string> npcs;

        /// <summary>
        /// A turn ordering over game characters.
        /// </summary>
        private List<string> turnOrder;
        public List<string> TurnOrder
        {
            get { return turnOrder; }
        }

        /// <summary>
        /// Stores all the edges of the tree.
        /// </summary>
        private Hashtable tree;
        public Hashtable Tree
        {
            get { return tree; }
            set { tree = value; }
        }

        /// <summary>
        /// The disk path where the tree is stored.
        /// </summary>
        private string path;
        public string Path
        {
            get { return path; }
            set { path = value; }
        }

        /// <summary>
        /// Returns the root node of the tree.
        /// </summary>
        public MediationTreeNode Root
        {
            get { return GetNode(0); }
        }

        /// <summary>
        /// The base constructor.
        /// </summary>
        /// <param name="domain">The base domain object.</param>
        /// <param name="problem">The base problem object.</param>
        /// <param name="path">The path used to save files to disk.</param>
        public MediationTree (Domain domain, Problem problem, string path)
        {
            // Store the domain, problem, and path.
            this.domain = domain;
            this.problem = problem;
            this.path = path;

            // Store the player's name.
            player = problem.Player.ToLower();

            // Get and store the NPC names.
            npcs = GetNPCs();

            // Establish the turn order.
            turnOrder = GetTurnOrder();

            // Create a new hashtable to store tree edges.
            tree = new Hashtable();

            // Initialize the session counters.
            nodeCounter = 0;
            lowestDepth = 0;
            goalStateCount = 0;
            deadEndCount = 0;

            // Create the tree's root node.
            MediationTreeNode root = CreateNode(domain, problem, null);
        }

        /// <summary>
        /// Checks if the current node is valid.
        /// </summary>
        /// <param name="id">The current node's ID.</param>
        /// <returns></returns>
        public bool ValidNode(int id)
        {
            // If the tree hashable contains the ID or it's the root, return true.
            return (tree.ContainsKey(id) || id == 0);
        }

        /// <summary>
        /// Get the parent ID of a node.
        /// </summary>
        /// <param name="id">The current node's ID.</param>
        /// <returns></returns>
        public int GetParent(int id)
        {
            // If the ID is valid, return its parent value.
            if (tree.ContainsKey(id)) return (int)tree[id];

            // Otherwise, return the error value.
            return -1;
        }

        /// <summary>
        /// Creates a new node object.
        /// </summary>
        /// <param name="domain">The node's domain.</param>
        /// <param name="problem">The node's problem.</param>
        /// <param name="incoming">The node's incoming edge.</param>
        /// <returns></returns>
        public MediationTreeNode CreateNode (Domain domain, Problem problem, MediationTreeEdge incoming)
        {
            // Create a placeholder for the new node object.
            MediationTreeNode node = null;

            // If the node is a root, initialize a root node.
            if (incoming == null) node = new MediationTreeNode(domain, problem, 0);
            // Otherwise, it is a child node...
            else
            {
                // Store the current node's ID in the incoming edge.
                incoming.Child = ++nodeCounter;

                // Create the new node object.
                node = new MediationTreeNode(domain, problem, incoming, GetSuccessorState(incoming), incoming.Child, GetDepth(incoming.Parent) + 1);

                // Add the edge to the tree hashtable.
                tree.Add(incoming.Child, incoming.Parent);
            }

            // If the node is a goal state, iterate the goal state counter.
            if (node.IsGoal) goalStateCount++;

            // If the node is at a lower depth than the previous record holder, update the depth counter.
            if (node.Depth > lowestDepth) lowestDepth = node.Depth;

            // Find and store the node's outgoing edges.
            node.Outgoing = GetOutgoingEdges(node, GetCurrentTurn(node));

            // Iterate through the node's outgoing edges.
            foreach (GameTreeEdge edge in node.Outgoing)
                // And add each of them to the unplayed collection.
                node.Unplayed.Add(edge);

            // Save the current node to disk.
            SetNode(node);

            // Return the current node object.
            return node;
        }

        /// <summary>
        /// Creates a turn ordering given a set of characters.
        /// </summary>
        /// <returns>A list of character names that represent the turn ordering.</returns>
        private List<string> GetTurnOrder()
        {
            // Initialize the character list.
            List<string> characters = new List<string>();

            // Loop through the computer controlled characters.
            foreach (string npc in npcs)
                // Add each to the list.
                characters.Add(npc);

            // Add the player to the list.
            characters.Add(player);

            // Return the list.
            return characters;
        }

        /// <summary>
        /// Get the current turn taker.
        /// </summary>
        /// <param name="node">The current node.</param>
        /// <returns>The name of the character who gets to act.</returns>
        private string GetCurrentTurn(GameTreeNode node)
        {
            // Returns the character who gets to act at this level.
            return turnOrder[node.Depth % turnOrder.Count];
        }

        /// <summary>
        /// Returns a list of outgoing edges for a given node.
        /// </summary>
        /// <param name="node">The node object.</param>
        /// <param name="actor">The name of the current actor.</param>
        /// <returns>A list of outgoing edges.</returns>
        public List<GameTreeEdge> GetOutgoingEdges(GameTreeNode node, string actor)
        {
            // Initialize the list of outgoing edges.
            List<GameTreeEdge> outgoing = new List<GameTreeEdge>();

            // Iterate through the actions enabled for the input actor in the current node's state.
            foreach (Operator action in StateSpaceTools.GetActions(actor, node.Domain, node.Problem, node.State))
                // Add an outgoing edge for each of these actions to the list.
                outgoing.Add(new GameTreeEdge(action, node.ID));

            // Return the list of outgoing edges.
            return outgoing;
        }

        /// <summary>
        /// Returns a list of NPC characters.
        /// </summary>
        /// <returns>A list of NPC names.</returns>
        private List<string> GetNPCs()
        {
            // Initialize the list.
            List<string> characters = new List<string>();

            // Iterate through the objects in the problem.
            foreach (Obj obj in problem.Objects)
                // If the object is not the player...
                if (!obj.Name.Equals(player))
                {
                    // A variable to remember whether a character was found.
                    bool found = false;

                    // If the object is of type character.
                    if (obj.SubType.Equals("character"))
                    {
                        // Add the character to the list.
                        characters.Add(obj.Name);

                        // Remember that a character was found.
                        found = true;
                    }

                    // If no character was found.
                    if (!found)
                        // Loop through all the super-types.
                        foreach (string type in obj.Types)
                            // If a super-type is character.
                            if (type.Equals("character"))
                            {
                                // Add the character.
                                characters.Add(obj.Name);

                                // Remember it was found.
                                found = true;

                                // Break from the loop;
                                break;
                            }

                    // If no character was found again.
                    if (!found)
                        // Loop through all the predicates in the initial state.
                        foreach (Predicate init in problem.Initial)
                            // Check if there is a predicate that lists the object as a character.
                            if (init.Name.Equals("character") && init.TermAtEquals(0, obj.Name))
                            {
                                // If so, add the character to the list.
                                characters.Add(obj.Name);

                                // And break the loop.
                                break;
                            }
                }

            // Return the list of characters.
            return characters;
        }

        /// <summary>
        /// Simulates a playout from the current node.
        /// </summary>
        /// <param name="id">The current node's ID.</param>
        /// <returns>Whether the playout was won or lost.</returns>
        public bool Simulate(int id)
        {
            // Read the node's object from disk.
            GameTreeNode node = GetNode(id);

            // Iterate the total plays.
            totalPlays++;

            // Initialize a dead end check.
            bool check = false;

            // If the node is not a dead end before it is played, we need to check.
            if (!node.DeadEnd) check = true;

            // Play a game and store the result.
            bool result = node.Play();

            // Store a win.
            if (result) totalWins++;
            // Otherwise, store a loss.
            else totalLosses++;

            // If we found a new dead end, record it.
            if (check && node.DeadEnd) deadEndCount++;

            // Save the node to disk.
            SetNode(node);

            // Return the result.
            return result;
        }

        /// <summary>
        /// Finds and returns the depth of the current node.
        /// </summary>
        /// <param name="id">The node's ID.</param>
        /// <returns>The depth of the current node.</returns>
        private int GetDepth(int id)
        {
            // Get the node object from disk and return its depth.
            return GetNode(id).Depth;
        }

        /// <summary>
        /// Given an edge, returns the successor state.
        /// </summary>
        /// <param name="edge">The edge object.</param>
        /// <returns>The new state after the action is taken.</returns>
        private State GetSuccessorState(GameTreeEdge edge)
        {
            // Apply the edge's action to the parent state and return the result.
            return GetNode(edge.Parent).State.NewState(edge.Action as Operator, problem.Objects);
        }

        /// <summary>
        /// Read a node object from disk.
        /// </summary>
        /// <param name="nodeID">The node's ID.</param>
        /// <returns>The node object.</returns>
        public GameTreeNode GetNode(int nodeID)
        {
            // Given the path and ID, deserializes the object from disk.
            return BinarySerializer.DeSerializeObject<GameTreeNode>(path + nodeID);
        }

        /// <summary>
        /// Write a node object to disk.
        /// </summary>
        /// <param name="node">The node's ID.</param>
        public void SetNode(GameTreeNode node)
        {
            // Given the path and ID, write the node object to disk.
            BinarySerializer.SerializeObject<GameTreeNode>(path + node.ID, node);
        }
    }
}
