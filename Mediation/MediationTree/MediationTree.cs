using Mediation.PlanTools;
using System;
using System.Collections;
using System.Collections.Generic;

using Mediation.StateSpace;
using Mediation.FileIO;
using Mediation.Utilities;
using Mediation.Enums;
using System.IO;
using Mediation.KnowledgeTools;
using Mediation.Interfaces;

namespace Mediation.MediationTree
{
    /// <summary>
    /// Records statistics and performs operations for maintaining game trees.
    /// </summary>
    [Serializable]
    public class MediationTree
    {
        private MediationTreeData data;

        public string Player
        {
            get { return data.player; }
        }

        public int LowestDepth
        {
            get { return data.lowestDepth; }
        }

        public int GoalStateCount
        {
            get { return data.goalStateCount; }
        }

        public int DeadEndCount
        {
            get { return data.deadEndCount; }
        }

        public int TotalNodes
        {
            get { return data.nodeCounter + 1; }
        }

        public List<string> TurnOrder
        {
            get { return data.turnOrder; }
        }

        public Hashtable Tree
        {
            get { return data.tree; }
            set
            {
                // Set the new links.
                data.tree = value;

                // Save the mediation tree.
                SaveTree();
            }
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
        private MediationTreeNode root;
        public MediationTreeNode Root
        {
            get
            {
                if (root == null) root = GetNode(0);
                return root;
            }
        }

        /// <summary>
        /// The base constructor.
        /// </summary>
        /// <param name="planner">The planner to use.</param>
        /// <param name="domain">The base domain object.</param>
        /// <param name="problem">The base problem object.</param>
        /// <param name="path">The path used to save files to disk.</param>
        public MediationTree (Domain domain, Problem problem, string path) : this (domain, problem, path, true, true, true) { }

        /// <summary>
        /// The base constructor.
        /// </summary>
        /// <param name="planner">The planner to use.</param>
        /// <param name="domain">The base domain object.</param>
        /// <param name="problem">The base problem object.</param>
        /// <param name="path">The path used to save files to disk.</param>
        /// <param name="domainRevision">Whether to use domain revision.</param>
        /// <param name="eventRevision">Whether to use event revision.</param>
        public MediationTree(Domain domain, Problem problem, string path, bool domainRevision, bool eventRevision) : this(domain, problem, path, domainRevision, eventRevision, false) { }

        /// <summary>
        /// The base constructor.
        /// </summary>
        /// <param name="planner">The planner to use.</param>
        /// <param name="domain">The base domain object.</param>
        /// <param name="problem">The base problem object.</param>
        /// <param name="path">The path used to save files to disk.</param>
        /// <param name="domainRevision">Whether to use domain revision.</param>
        /// <param name="eventRevision">Whether to use event revision.</param>
        /// <param name="superpositionManipulation">Whether to use superposition manipulation.</param>
        public MediationTree (Domain domain, Problem problem, string path, bool domainRevision, bool eventRevision, bool superpositionManipulation)
        {
            // Set the path.
            this.path = path;

            // Check each path to see if it exists. If not, create the folder.
            if (!File.Exists(path)) Directory.CreateDirectory(path);

            // If data already exists, load it from memory.
            if (File.Exists(path + "mediationtree")) data = BinarySerializer.DeSerializeObject<MediationTreeData>(path + "mediationtree");
            // Otherwise, initialize a new tree.
            else
            {
                data.eventRevision = eventRevision;
                data.domainRevision = domainRevision;
                data.superpositionManipulation = superpositionManipulation;

                // Store the domain and problem.
                data.domain = domain;
                data.problem = problem;

                // Store the player's name.
                data.player = problem.Player.ToLower();

                // Get and store the NPC names.
                data.npcs = GetNPCs();

                // Establish the turn order.
                data.turnOrder = GetTurnOrder();

                // Create a new hashtable to store tree edges.
                data.tree = new Hashtable();

                // Initialize the session counters.
                data.nodeCounter = 0;
                data.lowestDepth = 0;
                data.goalStateCount = 0;
                data.deadEndCount = 0;

                // Create the tree's root node.
                MediationTreeNode root = CreateNode(domain, problem, null);
            }
        }

        /// <summary>
        /// Checks if the current node is valid.
        /// </summary>
        /// <param name="id">The current node's ID.</param>
        /// <returns></returns>
        public bool ValidNode(int id)
        {
            // If the tree hashable contains the ID or it's the root, return true.
            return (data.tree.ContainsKey(id) || id == 0);
        }

        /// <summary>
        /// Get the parent ID of a node.
        /// </summary>
        /// <param name="id">The current node's ID.</param>
        /// <returns></returns>
        public int GetParent(int id)
        {
            // If the ID is valid, return its parent value.
            if (data.tree.ContainsKey(id)) return (int)data.tree[id];

            // Otherwise, return the error value.
            return -1;
        }

        /// <summary>
        /// The default Create Node behavior.
        /// </summary>
        /// <param name="domain">The node's domain.</param>
        /// <param name="problem">The node's problem.</param>
        /// <param name="incoming">The node's incoming edge.</param>
        /// <returns>A new tree node.</returns>
        private MediationTreeNode CreateNode (Domain domain, Problem problem, MediationTreeEdge incoming)
        {
            if (incoming != null) return CreateNode (domain, problem, incoming, GetNode(incoming.Parent).Plan);
            else return CreateNode(domain, problem, incoming, null);
        }

        /// <summary>
        /// Allows custom plans to be passed in.
        /// </summary>
        /// <param name="domain">The node's domain.</param>
        /// <param name="problem">The node's problem.</param>
        /// <param name="incoming">The node's incoming edge.</param>
        /// <returns>A new tree node.</returns>
        private MediationTreeNode CreateNode (Domain domain, Problem problem, MediationTreeEdge incoming, Plan plan)
        {
            // Create a placeholder for the new node object.
            MediationTreeNode node = null;

            // If the node is a root, initialize a root node.
            if (incoming == null)
            {
                if (!data.superpositionManipulation) node = new MediationTreeNode(domain, problem, 0);
                else node = new VirtualMediationTreeNode(domain, problem, 0);
            }
            // Otherwise, it is a child node...
            else
            {
                // Store the current node's ID in the incoming edge.
                incoming.Child = ++data.nodeCounter;

                if (!data.superpositionManipulation) node = new MediationTreeNode(domain, problem, incoming, GetSuccessorState(incoming), plan, incoming.Child, GetDepth(incoming.Parent) + 1);
                else node = new VirtualMediationTreeNode(domain, problem, incoming, GetSuccessorSuperposition(incoming), plan, incoming.Child, GetDepth(incoming.Parent) + 1);

                // Add the edge to the tree hashtable.
                data.tree[incoming.Child] = incoming.Parent;

                MediationTreeNode parent = GetNode(incoming.Parent);
                if (incoming.Action != null) parent.Outgoing.Find(x => x.Action.Equals(incoming.Action)).Child = node.ID;
                else
                {
                    foreach (MediationTreeEdge edge in parent.Outgoing)
                        if (edge is VirtualMediationTreeEdge)
                            if ((edge as VirtualMediationTreeEdge).Equals(incoming as VirtualMediationTreeEdge))
                                edge.Child = node.ID;
                }
                
                SetNode(parent);

                if (data.superpositionManipulation)
                {
                    Superposition super = node.State as Superposition;
                    super.States = SuperpositionManipulator.Collapse(data.player, node as VirtualMediationTreeNode, SuperpositionChooser.ChooseUtility);
                    node.State = super;
                }
            }

            // If the node is a goal state, iterate the goal state counter.
            if (node.IsGoal) data.goalStateCount++;

            // If the node is a dead end, iterate the dead end counter.
            if (node.DeadEnd)
            {
                if (data.eventRevision) EventRevisor.EventRevision(Planner.FastDownward, node, this);

                if (node.DeadEnd && data.domainRevision) DomainRevisor.DomainRevision(Planner.FastDownward, node, this);

                if (node.DeadEnd) data.deadEndCount++;
            }

            // If the node is at a lower depth than the previous record holder, update the depth counter.
            if (node.Depth > data.lowestDepth) data.lowestDepth = node.Depth;

            // If the node is not a dead end.
            if (!node.DeadEnd)
                // Find and store the node's outgoing edges.
                node.Outgoing = GetOutgoingEdges(node, GetCurrentTurn(node));

            // Save the current node to disk.
            SetNode(node);

            // Return the current node object.
            return node;
        }

        /// <summary>
        /// Creates a new node or returns the cached version.
        /// </summary>
        /// <param name="domain">The incoming domain.</param>
        /// <param name="problem">The incoming problem.</param>
        /// <param name="incoming">The incoming edge.</param>
        /// <returns>The child mediation tree node.</returns>
        public MediationTreeNode GetNode (Domain domain, Problem problem, MediationTreeEdge incoming)
        {
            // Return the cached child object if it has already been expanded.
            if (incoming.Child >= 0)
            {
                MediationTreeNode node = GetNode(incoming.Child);
                if (node != null) return node;
            }

            // Otherwise, return a new node.
            return CreateNode(domain, problem, incoming);
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
            foreach (string npc in data.npcs)
                // Add each to the list.
                characters.Add(npc);

            // Add the player to the list.
            characters.Add(data.player);

            // Return the list.
            return characters;
        }

        /// <summary>
        /// Get the current turn taker.
        /// </summary>
        /// <param name="node">The current node.</param>
        /// <returns>The name of the character who gets to act.</returns>
        private string GetCurrentTurn(MediationTreeNode node)
        {
            // Returns the character who gets to act at this level.
            return data.turnOrder[node.Depth % data.turnOrder.Count];
        }

        /// <summary>
        /// Get the turn taker at the given depth index.
        /// </summary>
        /// <param name="index">The tree depth index.</param>
        /// <returns>The name of the character who acts at the given level.</returns>
        public string GetTurnAtIndex(int index)
        {
            // Returns the character who gets to act at this level.
            return data.turnOrder[index % data.turnOrder.Count];
        }

        /// <summary>
        /// Returns a list of outgoing edges for a given node.
        /// </summary>
        /// <param name="node">The node object.</param>
        /// <param name="actor">The name of the current actor.</param>
        /// <returns>A list of outgoing edges.</returns>
        public List<MediationTreeEdge> GetOutgoingEdges (MediationTreeNode node, string actor)
        {
            List<MediationTreeEdge> outgoing = StateSpaceTools.GetAllPossibleActions(actor, node);
            if (!data.superpositionManipulation) return outgoing;

            List<MediationTreeEdge> unobservedActions = new List<MediationTreeEdge>();
            List<MediationTreeEdge> observedActions = new List<MediationTreeEdge>();
            foreach (MediationTreeEdge edge in outgoing)
            {
                Superposition super = node.State as Superposition;
                bool obs = false;
                foreach (State state in super.States)
                    if (state.Satisfies(edge.Action.Preconditions))
                        if (KnowledgeAnnotator.Observes(Player, edge.Action, state.Predicates))
                        {
                            observedActions.Add(edge);
                            obs = true;
                            break;
                        }

                if (!obs) unobservedActions.Add(edge);
            }

            if (unobservedActions.Count > 0)
            {
                VirtualMediationTreeEdge super = new VirtualMediationTreeEdge();
                foreach (MediationTreeEdge unobserved in unobservedActions)
                    super.Actions.Add(unobserved.Action as Operator);
                super.Parent = node.ID;
                observedActions.Add(super);
            }

            return observedActions;
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
            foreach (Obj obj in data.problem.Objects)
                // If the object is not the player...
                if (!obj.Name.Equals(data.player))
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
                        foreach (Predicate init in data.problem.Initial)
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
        public State GetSuccessorState (MediationTreeEdge edge)
        {
            // Apply the edge's action to the parent state and return the result.
            return GetNode(edge.Parent).State.NewState(edge.Action as Operator, data.problem.Objects);
        }

        public State GetSuccessorSuperposition (MediationTreeEdge edge)
        {
            VirtualMediationTreeNode parent = GetNode(edge.Parent) as VirtualMediationTreeNode;
            Superposition pred = parent.State as Superposition;
            Superposition super = new Superposition();
            if (edge is VirtualMediationTreeEdge)
            {
                VirtualMediationTreeEdge vEdge = edge as VirtualMediationTreeEdge;
                foreach (State state in pred.States)
                    foreach (Operator action in vEdge.Actions)
                        if (state.Satisfies(action.Preconditions))
                            super.States.Add(state.NewState(action, data.problem.Objects));
            }
            else
            {
                foreach (State state in pred.States)
                    if (state.Satisfies(edge.Action.Preconditions))
                        super.States.Add(state.NewState(edge.Action as Operator, data.problem.Objects));
            }

            return super;
        }

        /// <summary>
        /// Read a node object from disk.
        /// </summary>
        /// <param name="nodeID">The node's ID.</param>
        /// <returns>The node object.</returns>
        public MediationTreeNode GetNode (int nodeID)
        {
            // Given the path and ID, deserializes the object from disk.
            return BinarySerializer.DeSerializeObject<MediationTreeNode>(path + nodeID);
        }

        /// <summary>
        /// Write a node object to disk.
        /// </summary>
        /// <param name="node">The node's ID.</param>
        public void SetNode (MediationTreeNode node)
        {
            // Given the path and ID, write the node object to disk.
            BinarySerializer.SerializeObject<MediationTreeNode>(path + node.ID, node);

            // Save the mediation tree.
            SaveTree();
        }

        /// <summary>
        /// Writes the current tree data to disk.
        /// </summary>
        public void SaveTree ()
        {
            // Save the mediation tree.
            BinarySerializer.SerializeObject<MediationTreeData>(path + "mediationtree", data);
        }
    }
}
