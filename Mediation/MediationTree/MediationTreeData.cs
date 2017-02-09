using Mediation.PlanTools;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Mediation.MediationTree
{
    [Serializable]
    public struct MediationTreeData
    {
        public bool eventRevision;
        public bool domainRevision;

        /// <summary>
        /// Stores the current highest node ID.
        /// </summary>
        public int nodeCounter;

        /// <summary>
        /// Stores the lowest depth at which a node has been expanded.
        /// </summary>
        public int lowestDepth;

        /// <summary>
        /// Stores how many goal states have been reached by the tree.
        /// </summary>
        public int goalStateCount;

        /// <summary>
        /// Stores how many dead ends have been reached by the tree.
        /// </summary>
        public int deadEndCount;        

        /// <summary>
        /// The game tree's base domain object.
        /// </summary>
        public Domain domain;

        /// <summary>
        /// The game tree's base problem object.
        /// </summary>
        public Problem problem;

        /// <summary>
        /// The player's character.
        /// </summary>
        public string player;

        /// <summary>
        /// A list of the computer controlled characters.
        /// </summary>
        public List<string> npcs;

        /// <summary>
        /// A turn ordering over game characters.
        /// </summary>
        public List<string> turnOrder;
        

        /// <summary>
        /// Stores all the edges of the tree.
        /// </summary>
        public Hashtable tree;
    }
}
