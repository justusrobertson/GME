using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mediation.Interfaces;

namespace Mediation.PlanTools
{
    [Serializable]
    public class Obj : IObject
    {
        private string name;
        private string subType;
        private List<string> types;

        // Access the object's name.
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        // Access the object's sub-type.
        public string SubType
        {
            get { return subType; }
            set { subType = value; }
        }

        // Access the object's types.
        public List<string> Types
        {
            get { return types; }
            set { types = value; }
        }

        public Obj ()
        {
            name = "";
            subType = "";
            types = new List<string>();
        }

        public Obj(string name)
        {
            this.name = name;
            subType = "";
            types = new List<string>();
        }

        public Obj (string name, string subType)
        {
            this.name = name;
            this.subType = subType;
            types = new List<string>();
        }
        public Obj(string name, string subType, List<string> types)
        {
            this.name = name;
            this.subType = subType;
            this.types = types;
        }

        // Displays the object.
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(name);

            return sb.ToString();
        }

        // Creates a clone of the operator
        public Object Clone()
        {
            string newName = Name;

            string newSubType = subType;

            List<string> newTypes = new List<string>();
            foreach (string type in Types)
                newTypes.Add(type);

            return new Obj(newName, newSubType, newTypes);
        }
    }
}
