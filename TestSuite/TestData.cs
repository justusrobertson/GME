using Mediation.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace TestSuite
{
    [Serializable]
    public struct TestData
    {
        public long elapsedMilliseconds;
        public List<int> frontier;
        public int depth;

        /// <summary>
        /// Stores the current highest node ID.
        /// </summary>
        public int nodeCounter;

        /// <summary>
        /// Stores how many goal states have been reached by the tree.
        /// </summary>
        public int goalStateCount;

        /// <summary>
        /// Stores how many dead ends have been reached by the tree.
        /// </summary>
        public int deadEndCount;

        // Save the summaries of each build.
        public List<List<Tuple<String, String>>> summaries;
    }
}
