using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Mediation.Interfaces;

namespace Mediation.PlanTools
{
    [Serializable]
    public class Problem : IProblem
    {
        private string name;
        private string originalName;
        private string domain;
        private string player;
        private List<IObject> objects;
        private List<IPredicate> initial;
        private List<IIntention> intentions;
        private List<IPredicate> goal;
        private Hashtable typeList;
        private Hashtable objectsByType;

        // Access the problem's name.
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        // Access the problem's original name.
        public string OriginalName
        {
            get { return originalName; }
            set { originalName = value; }
        }

        // Access the problem's domain name.
        public string Domain
        {
            get { return domain; }
            set { domain = value; }
        }

        // Access the problem's player.
        public string Player
        {
            get 
            {
                if (player.Equals(""))
                    player = FindPlayer();

                return player;
            }
            set { player = value; }
        }

        // Access the problem's objects.
        public List<IObject> Objects
        {
            get { return objects; }
            set { objects = value; }
        }

        // Access the problem's initial state.
        public List<IPredicate> Initial
        {
            get { return initial; }
            set { initial = value; }
        }

        // Access the problem's intention list.
        public List<IIntention> Intentions
        {
            get { return intentions; }
            set { intentions = value; }
        }

        // Access the problem's goal state.
        public List<IPredicate> Goal
        {
            get { return goal; }
            set { goal = value; }
        }

        // Access the problem's type list.
        public Hashtable TypeList
        {
            get 
            {
                typeList = new Hashtable();

                foreach (IObject obj in objects)
                    if (typeList.ContainsKey(obj.SubType))
                    {
                        List<IObject> objList = typeList[obj.SubType] as List<IObject>;
                        objList.Add(obj);
                        typeList[obj.SubType] = objList;
                    }
                    else
                    {
                        List<IObject> objList = new List<IObject>();
                        objList.Add(obj);
                        typeList[obj.SubType] = objList;
                    }

                return typeList;
            }
        }

        // A hashtable that maps types to object names.
        public Hashtable ObjectsByType
        {
            get
            {
                if (objectsByType != null) return objectsByType;

                objectsByType = new Hashtable();

                foreach (IObject obj in objects)
                {
                    foreach (string type in obj.Types)
                    {
                        if (objectsByType.ContainsKey(type))
                        {
                            List<string> objList = objectsByType[type] as List<string>;
                            objList.Add(obj.Name);
                            objectsByType[type] = objList;
                        }
                        else
                        {
                            List<string> objList = new List<string>();
                            objList.Add(obj.Name);
                            objectsByType[type] = objList;
                        }
                    }

                    if (objectsByType.ContainsKey(obj.SubType))
                    {
                        List<string> objList = objectsByType[obj.SubType] as List<string>;
                        objList.Add(obj.Name);
                        objectsByType[obj.SubType] = objList;
                    }
                    else
                    {
                        List<string> objList = new List<string>();
                        objList.Add(obj.Name);
                        objectsByType[obj.SubType] = objList;
                    }
                }

                return objectsByType;
            }
        }

        public Problem ()
        {
            name = "";
            originalName = "";
            domain = "";
            player = "";
            objects = new List<IObject>();
            initial = new List<IPredicate>();
            intentions = new List<IIntention>();
            goal = new List<IPredicate>();
        }

        public Problem(string name, string originalName, string domain, string player, List<IObject> objects, List<IPredicate> initial, List<IPredicate> goal)
        {
            this.name = name;
            this.originalName = originalName;
            this.domain = domain;
            this.player = player;
            this.objects = objects;
            this.initial = initial;
            intentions = new List<IIntention>();
            this.goal = goal;
        }

        public Problem(string name, string originalName, string domain, string player, List<IObject> objects, List<IPredicate> initial, List<IIntention> intentions, List<IPredicate> goal)
        {
            this.name = name;
            this.originalName = originalName;
            this.domain = domain;
            this.player = player;
            this.objects = objects;
            this.initial = initial;
            this.intentions = intentions;
            this.goal = goal;
        }

        // Finds the player in the initial state.
        private string FindPlayer()
        {
            foreach (IPredicate pred in initial)
                if (pred.Name.Equals("player"))
                    return pred.TermAt(0).Constant;

            return "";
        }

        // Displays the contents of the problem.
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Problem " + name + " of Domain " + domain);

            for (int i = sb.ToString().Length - 1; i > 0; i--)
                sb.Append('-');

            sb.AppendLine();
            sb.AppendLine("Objects of Problem " + name);
            foreach (IObject obj in objects)
                sb.AppendLine(obj.Name);

            sb.AppendLine();
            sb.AppendLine("Initial State of Problem " + name);
            foreach (IPredicate pred in initial)
                sb.AppendLine(pred.ToString());

            sb.AppendLine();
            sb.AppendLine("Goal State of Problem " + name);
            foreach (IPredicate pred in goal)
                sb.AppendLine(pred.ToString());

            return sb.ToString();
        }

        // Creates a clone of the problem.
        public Object Clone()
        {
            // Create a new name object.
            string newName = name;

            // Create a new original name object.
            string newOriginalName = originalName;

            // Create a new domain name object.
            string newDomain = domain;

            // Create a new player name object.
            string newPlayer = player;

            // Create a new list of object objects.
            List<IObject> newObjects = new List<IObject>();
            foreach (IObject ob in objects)
                newObjects.Add(ob.Clone() as Obj);

            // Create a new list of intention objects.
            List<IIntention> newIntentions = new List<IIntention>();
            foreach (IIntention intention in intentions)
                newIntentions.Add(intention.Clone() as Intention);

            // Create a new list of initial state predicate objects.
            List<IPredicate> newInitial = new List<IPredicate>();
            foreach (IPredicate pred in initial)
                newInitial.Add(pred.Clone() as Predicate);

            // Create a new list of goal state predicate objects.
            List<IPredicate> newGoal = new List<IPredicate>();
            foreach (IPredicate pred in goal)
                newGoal.Add(pred.Clone() as Predicate);

            // Return the new domain object.
            return new Problem (newName, newOriginalName, newDomain, newPlayer, newObjects, newInitial, newIntentions, newGoal);
        }

        // Returns a hashcode.
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                // Suitable nullity checks etc, of course :)
                hash = hash * 23 + OriginalName.GetHashCode();
                hash = hash * 23 + Domain.GetHashCode();

                foreach (IObject obj in objects)
                    hash = hash * 23 + obj.GetHashCode();

                foreach (IIntention intention in intentions)
                    hash = hash * 23 + intention.GetHashCode();

                foreach (IPredicate pred in initial)
                    hash = hash * 23 + pred.GetHashCode();

                foreach (IPredicate pred in goal)
                    hash = hash * 23 + pred.GetHashCode();

                return hash;
            }
        }
    }
}
