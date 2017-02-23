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
        /// <summary>
        /// The number of milliseconds elapsed since search began.
        /// </summary>
        public long elapsedMilliseconds;

        /// <summary>
        /// The list of nodes on the search frontier.
        /// </summary>
        public List<int> frontier;

        /// <summary>
        /// To what depth level the tree has been generated.
        /// </summary>
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

        /// <summary>
        /// How many records to skip before taking a summary.
        /// </summary>
        public int summarySkip;

        /// <summary>
        /// The list of summaries.
        /// </summary>
        public List<List<Tuple<String, String>>> summaries;
    }
}
