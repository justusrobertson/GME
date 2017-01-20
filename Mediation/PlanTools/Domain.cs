using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Mediation.Interfaces;
using Mediation.Enums;

namespace Mediation.PlanTools
{
    public class Domain : IDomain
    {
        private string name;
        private PlanType type;
        private List<IOperator> operators;
        private Hashtable objectTypes;
        private Hashtable constantTypes;

        // Access the domain's name.
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        // Access the domain's type.
        public PlanType Type
        {
            get { return type; }
            set { type = value; }
        }

        // Access the domain's operators.
        public List<IOperator> Operators
        {
            get { return operators; }
            set { operators = value; }
        }

        // Domains have a list of object types.
        public List<string> ObjectTypes
        {
            get 
            {
                List<string> types = new List<string>();

                foreach (string type in objectTypes.Keys)
                    types.Add(type);

                return types;
            }
        }

        // Domains have a list of constant types.
        public List<string> ConstantTypes
        {
            get
            {
                List<string> types = new List<string>();

                foreach (string type in constantTypes.Keys)
                    types.Add(type);

                return types;
            }
        }

        public Domain ()
        {
            name = "";
            type = new PlanType();
            operators = new List<IOperator>();
            objectTypes = new Hashtable();
            constantTypes = new Hashtable();
        }

        // Object type/sub-type pairs can be added to the domain.
        public void AddTypePair(string subType, string type)
        {
            // Check if the type already has children.
            if (!objectTypes.ContainsKey(type))
                // If not, create an entry and add the sub-type.
                objectTypes.Add(type, new List<string> { subType });
            else
            {
                // Otherwise, pull the existing entry from the hashtable.
                List<string> typeList = objectTypes[type] as List<string>;

                // Add the sub-type to the existing entry.
                typeList.Add(subType);

                // Push the updated entry back to the hashtable.
                objectTypes[type] = typeList;
            }
        }

        // Object type/sub-type lists can be added to the domain.
        public void AddTypeList(List<string> subTypes, string type)
        {
            // Check if the type already has children.
            if (!objectTypes.ContainsKey(type))
                // If not, create an entry and add the sub-types.
                objectTypes.Add(type, subTypes);
            else
            {
                // Otherwise, pull the existing entry from the hashtable.
                List<string> typeList = objectTypes[type] as List<string>;

                // Add the sub-type to the existing entry.
                typeList.AddRange(subTypes);

                // Push the updated entry back to the hashtable.
                objectTypes[type] = typeList;
            }
        }

        // The domain has a list of all sub-types associated with a type.
        public List<string> GetSubTypesOf(string type)
        {
            // Return all sub-types associated with the given type.
            if (objectTypes.ContainsKey(type))
                return objectTypes[type] as List<string>;
            
            return new List<string>();
        }

        // Object type/constant pairs can be added to the domain.
        public void AddConstantPair(string constant, string type)
        {
            // Check if the type already has children.
            if (!constantTypes.ContainsKey(type))
                // If not, create an entry and add the sub-type.
                constantTypes.Add(type, new List<string> { constant });
            else
            {
                // Otherwise, pull the existing entry from the hashtable.
                List<string> typeList = constantTypes[type] as List<string>;

                // Add the sub-type to the existing entry.
                typeList.Add(constant);

                // Push the updated entry back to the hashtable.
                constantTypes[type] = typeList;
            }
        }

        // Object type/constant lists can be added to the domain.
        public void AddConstantsList(List<string> constants, string type)
        {
            // Check if the type already has children.
            if (!constantTypes.ContainsKey(type))
                // If not, create an entry and add the sub-types.
                constantTypes.Add(type, constants);
            else
            {
                // Otherwise, pull the existing entry from the hashtable.
                List<string> typeList = constantTypes[type] as List<string>;

                // Add the sub-type to the existing entry.
                typeList.AddRange(constants);

                // Push the updated entry back to the hashtable.
                constantTypes[type] = typeList;
            }
        }

        // The domain has a list of all sub-types associated with a type.
        public List<string> GetConstantsOf(string type)
        {
            // Return all sub-types associated with the given type.
            if (constantTypes.ContainsKey(type))
                return constantTypes[type] as List<string>;

            return new List<string>();
        }

        // Displays the contents of the domain.
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Domain " + name);

            foreach (Operator op in operators)
                sb.AppendLine(op.ToString());

            return sb.ToString();
        }
    }
}
