using Mediation.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mediation.Utilities
{
    public class PredicateListComparer : IEqualityComparer<List<IPredicate>>
    {
        /// <summary>
        /// Returns whether two lists of literals are equal.
        /// </summary>
        /// <param name="x">The first set of literals.</param>
        /// <param name="y">The second set of literals.</param>
        /// <returns></returns>
        public bool Equals (List<IPredicate> x, List<IPredicate> y)
        {
            // Loop through each literal in the first list.
            foreach (IPredicate pred in x)
                // If the literal is not present in the second list, return false.
                if (!y.Contains(pred)) return false;

            // Loop through each literal in the second list.
            foreach (IPredicate pred in y)
                // If the literal is not present in the first list, return false.
                if (!x.Contains(pred)) return false;

            return true;
        }

        public int GetHashCode(List<IPredicate> obj)
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;

                foreach (IPredicate p in obj)
                {
                    // Suitable nullity checks etc, of course :)
                    hash = hash * 23 + p.Name.GetHashCode();

                    foreach (ITerm term in p.Terms)
                        hash = hash * 23 + term.GetHashCode();
                }

                return hash;
            }
        }
    }
}
