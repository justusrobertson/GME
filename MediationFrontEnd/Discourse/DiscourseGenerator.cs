using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mediation.Interfaces;
using Mediation.PlanTools;
using Mediation.KnowledgeTools;

namespace MediationFrontEnd.Discourse
{
    public static class DiscourseGenerator
    {
        public static string RoomDescription (string player, List<IPredicate> state, List<IObject> probObs)
        {
            string description = "";
            probObs.RemoveAll(obj => !KnowledgeAnnotator.Observes(player, obj, state));
            state.RemoveAll(pred => !pred.Observing(player));

            if (state.Count > 0)
                description += "You are standing in the " + KnowledgeAnnotator.GetLocation(player, state) + ". ";

            foreach (string character in GetCharacters(state, probObs))
                if (!character.Equals(player))
                {
                    description += "You see " + UppercaseFirst(character) + ". ";
                    List<string> properties = GetProperties(character, state);
                    if (properties.Count == 1)
                        description += UppercaseFirst(character) + " is " + properties.First() + ". ";
                    if (properties.Count == 2)
                        description += UppercaseFirst(character) + " is " + properties.First() + " and " + properties.Last() + ". ";
                    else if (properties.Count > 2)
                    {
                        description += UppercaseFirst(character) + " is ";
                        for (int i = 0; i < properties.Count - 1; i++)
                            description += properties[i] + ", ";
                        description += "and " + properties.Last() + ". ";
                    }

                    List<string> objects = GetObjectsByCharacter(character, state);
                    if (objects.Count == 1)
                        description += UppercaseFirst(character) + " has the " + UppercaseFirst(objects.First()) + ". ";
                    if (objects.Count == 2)
                        description += UppercaseFirst(character) + " has " + UppercaseFirst(objects.First()) + " and a " + UppercaseFirst(objects.Last()) + ". ";
                    else if (objects.Count > 2)
                    {
                        description += UppercaseFirst(character) + " has ";
                        for (int i = 0; i < objects.Count - 1; i++)
                            description += UppercaseFirst(objects[i]) + ", ";
                        description += "and a " + UppercaseFirst(objects.Last() + ". ");
                    }

                    foreach (string obj in objects)
                    {
                        properties = GetProperties(obj, state);
                        if (properties.Count == 1)
                            description += "The " + UppercaseFirst(obj) +" is " + properties.First() + ". ";
                        if (properties.Count == 2)
                            description += "The " + UppercaseFirst(obj) + " is " + properties.First() + " and " + properties.Last() + ". ";
                        else if (properties.Count > 2)
                        {
                            description += "The " + UppercaseFirst(obj) + " is ";
                            for (int i = 0; i < properties.Count - 1; i++)
                                description += properties[i] + ", ";
                            description += "and " + properties.Last() + ". ";
                        }
                    }
                }

            foreach (string obj in GetObjectsByRoom(state, probObs))
            {
                description += "You see " + obj + ". ";
                List<string> properties = GetProperties(obj, state);
                if (properties.Count == 1)
                    description += "The " + obj + " is " + properties.First() + ". ";
                if (properties.Count == 2)
                    description += "" + obj + " is a " + properties.First() + " and is " + properties.Last() + ". ";
                else if (properties.Count > 2)
                {
                    description += "The " + obj + " is ";
                    for (int i = 0; i < properties.Count - 1; i++)
                        description += properties[i] + ", ";
                    description += "and " + properties.Last() + ". ";
                }

                List<string> objects = GetObjectsByObject(obj, state);
                if (objects.Count == 1)
                    description += UppercaseFirst(objects.First()) + " is in " + UppercaseFirst(obj) + ". ";
                if (objects.Count == 2)
                    description += UppercaseFirst(objects.First()) + " and " + objects.Last() + " are in " + UppercaseFirst(obj) + ". ";
                else if (objects.Count > 2)
                {
                    for (int i = 0; i < objects.Count - 1; i++)
                        description += UppercaseFirst(objects[i]) + ", ";
                    description += "and " + UppercaseFirst(objects.Last()) + " are in " + UppercaseFirst(obj) + ". ";
                }
            }

            List<string> rooms = GetConnectedRooms(KnowledgeAnnotator.GetLocation(player, state), state);
            if (rooms.Count == 1)
                description += "You see a door leading to the " + UppercaseFirst(rooms.First()) + ". ";
            if (rooms.Count == 2)
                description += "You see doors leading to the " + UppercaseFirst(rooms.First()) + " and the " + UppercaseFirst(rooms.Last()) + ". ";
            else if (rooms.Count > 2)
            {
                description += "You see doors leading to the ";
                for (int i = 0; i < rooms.Count - 1; i++)
                    description += UppercaseFirst(rooms[i]) + ", ";
                description += "and the " + UppercaseFirst(rooms.Last()) + ". ";
            }

            List<string> locked = GetLockedRooms(KnowledgeAnnotator.GetLocation(player, state), rooms, state);
            if (locked.Count == 1)
                description += locked.First() + " is locked. ";
            if (locked.Count == 2)
                description += locked.First() + " and " + locked.Last() + " are locked. ";
            else if (locked.Count > 2)
            {
                for (int i = 0; i < locked.Count - 1; i++)
                    description += locked[i] + ", ";
                description += "and " + locked.Last() + " are locked. ";
            }

            return description;
        }

        public static string PlayerDescription (string player, List<IPredicate> state)
        {
            string description = "";
            state.RemoveAll(pred => !pred.Observing(player));

            description += "Your name is " + player + ". ";
            description += "You are standing in the " + KnowledgeAnnotator.GetLocation(player, state) + ". ";

            List<string> properties = GetProperties(player, state);
            if (properties.Count == 1)
                description += "You are " + properties.First() + ". ";
            if (properties.Count == 2)
                description += "You are " + properties.First() + " and " + properties.Last() + ". ";
            else if (properties.Count > 2)
            {
                description += "You are ";
                for (int i = 0; i < properties.Count - 1; i++)
                    description += properties[i] + ", ";
                description += "and " + properties.Last() + ". ";
            }

            List<string> objects = GetObjectsByCharacter(player, state);
            if (objects.Count == 1)
                description += "You have " + objects.First() + ". ";
            if (objects.Count == 2)
                description += "You have " + objects.First() + " and a " + objects.Last() + ". ";
            else if (objects.Count > 2)
            {
                description += "You have ";
                for (int i = 0; i < objects.Count - 1; i++)
                    description += objects[i] + ", ";
                description += "and a " + objects.Last() + ". ";
            }

            foreach (string obj in objects)
            {
                properties = GetProperties(obj, state);
                if (properties.Count == 1)
                    description += "The " + obj + " is " + properties.First() + ". ";
                if (properties.Count == 2)
                    description += "The " + obj + " is " + properties.First() + " and " + properties.Last() + ". ";
                else if (properties.Count > 2)
                {
                    description += "The " + obj + " is ";
                    for (int i = 0; i < properties.Count - 1; i++)
                        description += properties[i] + ", ";
                    description += "and " + properties.Last() + ". ";
                }
            }

            return description;
        }

        public static List<string> GetCharacters(List<IPredicate> state, List<IObject> probObs)
        {
            List<string> characters = new List<string>();

            foreach (IPredicate pred in state)
                if (pred.Name.Equals("character"))
                    characters.Add(pred.TermAt(0).Constant);

            foreach (IObject obj in probObs)
                if (obj.SubType.Equals("character"))
                    if (!characters.Contains(obj.Name))
                        characters.Add(obj.Name);

            return characters;
        }

        public static List<string> GetObjectsByCharacter (string character, List<IPredicate> state)
        {
            List<string> objects = new List<string>();

            foreach (IPredicate pred in state)
                if (pred.Name.Equals("has") && pred.TermAtEquals(0, character))
                    objects.Add(pred.TermAt(1).Constant);

            return objects;
        }

        public static List<string> GetObjectsByRoom(List<IPredicate> state, List<IObject> probObs)
        {
            List<string> objects = new List<string>();
            List<string> characters = GetCharacters(state, probObs);

            foreach (IPredicate pred in state)
                if (pred.Name.Equals("at") && !characters.Exists(character => character.Equals(pred.Terms.First().ToString())))
                    objects.Add(pred.Terms.First().Constant);

            return objects;
        }

        public static List<string> GetObjectsByObject(string obj, List<IPredicate> state)
        {
            List<string> objects = new List<string>();

            foreach (IPredicate pred in state)
                if (pred.Name.Equals("in") && pred.TermAtEquals(1, obj))
                    objects.Add(pred.Terms.First().Constant);

            return objects;
        }

        public static List<string> GetConnectedRooms(string room, List<IPredicate> state)
        {
            List<string> rooms = new List<string>();

            foreach (IPredicate pred in state)
                if (pred.Name.Equals("connected") && pred.TermAtEquals(0, room))
                    rooms.Add(pred.TermAt(1).Constant);

            return rooms;
        }

        public static List<string> GetLockedRooms(string room, List<string> rooms, List<IPredicate> state)
        {
            List<string> locked = new List<string>();

            foreach (IPredicate pred in state)
                if (pred.Name.Equals("locked"))
                    foreach (string rm in rooms)
                         if(pred.TermAtEquals(0, room) && pred.TermAtEquals(1, rm))
                            locked.Add(room);

            return locked;
        }

        public static List<string> GetProperties(string thing, List<IPredicate> state)
        {
            List<string> properties = new List<string>();

            foreach (IPredicate pred in state)
                if (pred.Arity == 1 && pred.TermAtEquals(0, thing) && !pred.Name.Equals("character") && !pred.Name.Equals("player") && !pred.Name.Equals("alive") && !pred.Name.Equals("object") && !pred.Name.Equals(thing))
                    properties.Add(pred.Name);

            return properties;
        }

        static string UppercaseFirst(string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }
    }
}
