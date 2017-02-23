using Mediation.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mediation.Utilities
{
    public class PredicateListComparer : IEqualityComparer<List<IPredicate>>
    {
        public bool Equals (List<IPredicate> x, List<IPredicate> y)
        {
            foreach (IPredicate pred in x)
                if (!y.Contains(pred)) return false;

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
