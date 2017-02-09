using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mediation.Interfaces;

namespace Mediation.PlanTools
{
    [Serializable]
    class Intention : IIntention
    {
        private string character;
        private IPredicate predicate;

        // Access the character's name.
        public string Character
        {
            get { return character; }
            set { character = value; }
        }

        // Access the character's intention.
        public IPredicate Predicate
        {
            get { return predicate; }
            set { predicate = value; }
        }

        public Intention ()
        {
            character = "";
            predicate = new Predicate();
        }

        public Intention(string character, IPredicate predicate)
        {
            this.character = character;
            this.predicate = predicate;
        }

        // Creates a clone of the intention.
        public Object Clone ()
        {
            string newCharacter = Character;

            IPredicate newPredicate = Predicate;

            return new Intention(newCharacter, newPredicate);
        }

        // Displays the intention.
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(character + " intends " + predicate);

            return sb.ToString();
        }
    }
}
