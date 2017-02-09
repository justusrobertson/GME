using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Mediation.Interfaces;
using Mediation.PlanTools;
using System.Collections;

namespace Mediation.KnowledgeTools
{
    public static class KnowledgeAnnotator
    {
        public static List<IPredicate> PredicateBinder (Hashtable objectTypes, IPredicate predicate, int termLocation)
        {
            List<IPredicate> predicateList = new List<IPredicate>();

            if (termLocation >= predicate.Terms.Count) return new List<IPredicate> { predicate };

            if (!objectTypes.ContainsKey(predicate.TermAt(termLocation).Type)) return new List<IPredicate>();

            foreach (string obj in objectTypes[predicate.TermAt(termLocation).Type] as List<string>)
            {
                Predicate newPredicate = predicate.Clone() as Predicate;
                newPredicate.BindTerm(obj, termLocation);
                predicateList.AddRange(PredicateBinder(objectTypes, newPredicate, termLocation + 1));
            }

            return predicateList;
        }

        // Returns the predicates that the character is currently observing.
        public static List<IPredicate> FullKnowledgeState (List<IPredicate> predicates, Hashtable objectTypes, List<IPredicate> state, string character)
        {
            List<IPredicate> possiblePredicates = new List<IPredicate>();
            foreach (IPredicate pred in predicates)
                possiblePredicates.AddRange(PredicateBinder(objectTypes, pred, 0));
            List<IPredicate> observedPossibles = KnowledgeState(possiblePredicates, state, character);
            Hashtable stateChecker = new Hashtable();
            foreach (IPredicate pred in state)
                stateChecker[pred.ToString()] = true;
            foreach (IPredicate obs in observedPossibles)
                if (!stateChecker.ContainsKey(obs.ToString()))
                    obs.Sign = false;
            return observedPossibles;
        }

        // Returns the predicates that the character could possibly observe of those in the check list given the state.
        public static List<IPredicate> KnowledgeState (List<IPredicate> check, List<IPredicate> state, string character)
        {
            // Create a new list.
            List<IPredicate> newState = new List<IPredicate>();

            // Foreach predicate, check to see if it is observed by the user.
            foreach (IPredicate pred in check)
                if (Observes(character, pred, state))
                    newState.Add((Predicate)pred.Clone());

            // Return the filtered predicates.
            return newState;
        }

        // Returns the predicates that the character is currently observing.
        public static List<IPredicate> KnowledgeState (List<IPredicate> state, string character)
        {
            return KnowledgeState (state, state, character);
        }

        // Marks the predicates that the character is currently observing.
        public static List<IPredicate> Annotate (List<IPredicate> state, string character)
        {
            // Create a new list.
            List<IPredicate> newState = new List<IPredicate>();

            // Populate the list with clones.
            foreach (IPredicate pred in state)
                newState.Add((Predicate)pred.Clone());

            // Foreach predicate, check to see if it is observed by the user.
            foreach (IPredicate pred in newState)
                if (Observes(character, pred, state))
                    pred.Observes(character, true);
                else
                    pred.Observes(character, false);

            // Return the annotated predicates.
            return newState;
        }

        // Checks to see if a predicate is currently observed.
        public static bool Observes (string character, IPredicate pred, List<IPredicate> state)
        {
            // If the character is at the same location as the predicate, return true.
            if (GetLocation(character, state).Equals(GetLocation(pred.TermAt(0).Constant, state)) 
                || GetLocation(character, state).Equals(pred.TermAt(0).Constant)
                || (pred.Name.Equals("at") && GetLocation(character, state).Equals(pred.TermAt(1).Constant)))
                return true;

            // Otherwise return false.
            return false;
        }

        // Checks to see if a object is currently observed.
        public static bool Observes (string character, IObject obj, List<IPredicate> state)
        {
            // If the character is at the same location as the predicate, return true.
            if (GetLocation(character, state).Equals(GetLocation(obj.Name, state)) || GetLocation(character, state).Equals(obj.Name))
                return true;

            // Otherwise return false.
            return false;
        }

        // Checks to see if an action is observed.
        public static bool Observes (string character, IOperator action, List<IPredicate> state)
        {
            // If the character and the action's actor are colocated, return true.
            if (GetLocation(character, state).Equals(GetLocation(action.Actor, state))) return true;

            // Otherwise, return false.
            return false;
        }

        // Returns the location of an object given a state.
        public static string GetLocation (string obj, List<IPredicate> state)
        {
            // Loop through the predicates in the state.
            foreach (Predicate pred in state)
                // Depending on the predicate type, return the correct location.
                if (pred.Name.Equals("at") && pred.TermAtEquals(0, obj))
                {
                    return pred.TermAt(1).Constant;
                }
                else if (pred.Name.Equals("in") && pred.TermAtEquals(0, obj))
                {
                    foreach (Predicate p in state)
                        if (p.Name.Equals("open") && p.TermAtEquals(0, pred.TermAt(1).Constant))
                            return GetLocation(pred.TermAt(1).Constant, state);
                }
                else if (pred.Name.Equals("location") && pred.TermAtEquals(0, obj))
                {
                    return pred.TermAt(0).Constant;
                }
                else if (pred.Name.Equals("room") && pred.TermAtEquals(0, obj))
                {
                    return pred.TermAt(0).Constant;
                }
                else if (pred.Name.Equals("has") && pred.TermAtEquals(1, obj))
                {
                    return GetLocation(pred.TermAt(0).Constant, state);
                }
                else if (pred.Name.Equals("on") && pred.TermAtEquals(0, obj))
                {
                    return GetLocation(pred.TermAt(1).Constant, state);
                }

            // If nothing is found, return a null location.
            return "";
        }
    }
}
