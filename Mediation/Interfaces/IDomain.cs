using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Mediation.Enums;

namespace Mediation.Interfaces
{
    public interface IDomain
    {
        // Domains have a name.
        string Name { get; set; }

        // Domains have a plan type.
        PlanType Type { get; set; }

        // Domains have a list of actions.
        List<IOperator> Operators { get; set; }

        // Domains have a list of object types.
        List<string> ObjectTypes { get; }

        // Domains have a list of constant types.
        List<string> ConstantTypes { get; }

        // Domains have a list of predicates.
        List<IPredicate> Predicates { get; set; }

        // Object type/sub-type pairs can be added to the domain.
        void AddTypePair (string subType, string type);

        // Object type/sub-type lists can be added to the domain.
        void AddTypeList(List<string> subTypes, string type);

        // The domain has a list of all sub-types associated with a type.
        List<string> GetSubTypesOf (string type);

        // Object type/contant pairs can be added to the domain.
        void AddConstantPair(string constant, string type);

        // Object type/constant lists can be added to the domain.
        void AddConstantsList(List<string> subTypes, string type);

        // The domain has a list of all constants associated with a type.
        List<string> GetConstantsOf(string type);

        // Domains can be cloned.
        Object Clone();
    }
}
