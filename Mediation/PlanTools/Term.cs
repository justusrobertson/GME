using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mediation.Interfaces;

namespace Mediation.PlanTools
{
    [Serializable]
    public class Term : ITerm
    {
        private string variable;
        private string constant;
        private string type;

        // Terms may have a variable symbol.
        public string Variable 
        {
            get { return variable; }
            set { variable = value; }
        }

        // Terms may have a constant symbol.
        public string Constant 
        {
            get { return constant; }
            set { constant = value; }
        }

        // Terms may have an associated type.
        public string Type 
        {
            get { return type; }
            set { type = value; }
        }

        // Terms may be bound or unbound.
        public bool Bound 
        {
            get 
            {
                if (constant.Equals(""))
                    return false;

                return true;
            }
        }

        public Term ()
        {
            variable = "";
            constant = "";
            type = "";
        }

        public Term (string variable)
        {
            this.variable = variable;
            constant = "";
            type = "";
        }

        public Term (string constant, bool bound)
        {
            variable = "";
            this.constant = constant;
            type = "";
        }

        public Term(string variable, string constant)
        {
            this.variable = variable;
            this.constant = constant;
            type = "";
        }

        public Term (string variable, string constant, string type)
        {
            this.variable = variable;
            this.constant = constant;
            this.type = type;
        }

        // Checks if two terms are equal.
        public override bool Equals(Object obj)
        {
            // Store the object as a Term.
            Term term = obj as Term;

            if (term.Bound && Bound)
                if (term.Constant.Equals(constant))
                    return true;

            if (!term.Bound && !Bound)
                if (term.Variable.Equals(variable))
                    return true;

            return false;
        }

        // Return a clone of the term.
        public object Clone ()
        {
            return new Term(variable, constant, type);
        }

        // Returns a hashcode.
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;

                // Suitable nullity checks etc, of course :)
                if (!Bound)
                    hash = hash * 23 + variable.GetHashCode();
                else
                    hash = hash * 23 + constant.GetHashCode();

                return hash;
            }
        }

        // Displays the term.
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (Bound)
                sb.Append(constant);
            else
                sb.Append(variable);

            return sb.ToString();
        }
    }
}
