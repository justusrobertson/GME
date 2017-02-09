using Mediation.Utilities;

namespace Mediation.GameTree
{
    public static class MCTS
    {
        /// <summary>
        /// Given a number of plays and a game tree object, performs MCTS.
        /// </summary>
        /// <param name="plays">The number of MCTS plays to perform.</param>
        /// <param name="tree">The game tree object on which to perform the search.</param>
        public static void Search (int plays, GameTree tree)
        {
            // Loop for the number of input play throughs.
            for (int i = 0; i < plays; i++)
            {
                // Select a leaf node.
                int select = Select(0, tree);

                // Expand the leaf node.
                int expand = Expand(select, tree);

                // Simulate a roll out from the leaf node.
                bool result = Simulate(expand, tree);

                // Propagate the result of the roll out back up the tree.
                Propagate(result, expand, tree);
            }
        }

        /// <summary>
        /// Selects a leaf node to expand.
        /// </summary>
        /// <param name="id">The ID of the current node.</param>
        /// <param name="tree">The game tree object.</param>
        /// <returns></returns>
        private static int Select (int id, GameTree tree)
        {
            // Get the node object that corresponds to the current ID.
            GameTreeNode node = tree.GetNode(id);

            // If the node has not been played before, has at least one outgoing edge, is not a dead end, and is not a goal state...
            if (node.Unplayed.Count == 0 && node.Outgoing.Count > 0 && !node.DeadEnd && !node.IsGoal)
            {
                // Create a variable to store the child with the highest interval.
                int highestChild = -1;

                // If it's not the player's turn...
                if (!tree.TurnOrder[node.Depth % tree.TurnOrder.Count].Equals(tree.Player))
                {
                    // Set the highest child to be the first child.
                    highestChild = node.Outgoing[0].Child;

                    // Loop through every remaining child.
                    for (int i = 1; i < node.Outgoing.Count; i++)
                        // If the current child has a higher interval than the stored child.
                        if (tree.GetInterval(node.Outgoing[i].Child) > tree.GetInterval(highestChild))
                            // Store the current child.
                            highestChild = node.Outgoing[i].Child;
                }
                // If it's the player's turn, chose an outgoing edge at random.
                else highestChild = node.Outgoing.PickRandom<GameTreeEdge>().Child;

                // Recursively call this method with the selected child.
                return Select(highestChild, tree);
            }

            // Return the leaf node.
            return node.ID;
        }

        /// <summary>
        /// Expands a selected node, if possible.
        /// </summary>
        /// <param name="id">The selected node.</param>
        /// <param name="tree">The game tree object.</param>
        /// <returns></returns>
        private static int Expand (int id, GameTree tree)
        {
            // Get the node object that corresponds with the input ID.
            GameTreeNode node = tree.GetNode(id);

            // If the node has at least one unplayed child, is not a dead end, and is not a goal state.
            if (node.Unplayed.Count > 0 && !node.DeadEnd && !node.IsGoal)
            {
                // Choose an unplayed child at random.
                GameTreeEdge outgoing = node.Unplayed.PickRandom<GameTreeEdge>();

                // Remove the child from the list of unplayed edges.
                node.Unplayed.Remove(outgoing);

                // Create the node object and store the child's ID.
                int child = tree.CreateNode(node.Domain, node.Problem, outgoing).ID;

                // Save the parent node to disk.
                tree.SetNode(node);

                // Return the child ID.
                return child;
            }

            // Return the leaf node.
            return node.ID;
        }

        /// <summary>
        /// Given a node, simulate a roll out.
        /// </summary>
        /// <param name="id">The ID of the selected node.</param>
        /// <param name="tree">The game tree object.</param>
        /// <returns></returns>
        private static bool Simulate (int id, GameTree tree)
        {
            // Ask the tree to simulate a roll out and return the result.
            return tree.Simulate(id);
        }

        /// <summary>
        /// Propagates a roll out's result back through the tree.
        /// </summary>
        /// <param name="result">Whether the roll out resulted in a win or loss.</param>
        /// <param name="node">The current node.</param>
        /// <param name="tree">The game tree object.</param>
        private static void Propagate (bool result, int node, GameTree tree)
        {
            // Get the current node's parent ID.
            int parent = tree.GetParent(node);

            // Loop until we hit the root node's null parent link.
            while (parent != -1)
            {
                // Get the parent's node object from the game tree.
                GameTreeNode parentNode = tree.GetNode(parent);

                // Add the win/loss to the parent's node object.
                parentNode.AddResult(result);

                // Save the parent's node object to disk.
                tree.SetNode(parentNode);

                // Set the parent ID to the grandparent ID.
                parent = tree.GetParent(parent);
            }
        }
    }
}
