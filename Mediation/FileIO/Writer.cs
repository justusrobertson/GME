using System;
using System.Collections.Generic;
using System.IO;

using Mediation.Interfaces;
using Mediation.PlanTools;
using Mediation.StateSpace;
using Mediation.GameTree;
using Mediation.PlanSpace;
using Mediation.Utilities;

namespace Mediation.FileIO
{
    public static class Writer
    {
        // Given a state, creates a problem PDDL file.
        public static void ProblemToPDDL (string file, Domain domain, Problem problem, List<IPredicate> state)
        {
            using (StreamWriter writer = new StreamWriter(file, false))
            {
                writer.WriteLine("(define (problem rob)");
                writer.WriteLine("(:domain " + domain.Name + ")");
                writer.Write("(:objects");
                if (!problem.Objects[0].SubType.Equals(""))
                    foreach (string type in problem.TypeList.Keys)
                    {
                        List<IObject> objects = problem.TypeList[type] as List<IObject>;
                        for (int i = 0; i < objects.Count; i++)
                        {
                            writer.Write(" " + objects[i].Name);
                            if (i == objects.Count - 1)
                                writer.WriteLine(" - " + type);
                        }
                    }
                else
                    foreach (IObject obj in problem.Objects)
                        writer.Write(" " + obj.Name);
                writer.WriteLine(")");
                writer.Write("(:init");
                foreach (IPredicate pred in state)
                    writer.WriteLine(" " + pred);
                foreach (IIntention intent in problem.Intentions)
                    writer.WriteLine(" (intends " + intent.Character + " " + intent.Predicate + ")");
                writer.WriteLine(")");
                if (problem.Goal.Count > 1)
                    writer.Write("(:goal (AND");
                else
                    writer.Write("(:goal");
                foreach (IPredicate pred in problem.Goal)
                    writer.WriteLine(" " + pred);
                if (problem.Goal.Count > 1)
                    writer.Write(")))");
                else
                    writer.Write("))");
            }
        }

        // Creates a domain PDDL file.
        public static void DomainToPDDL(string file, Domain domain)
        {
            using (StreamWriter writer = new StreamWriter(file, false))
            {
                writer.WriteLine("(define");
                writer.WriteLine("\t(domain " + domain.Name + ")");
                writer.WriteLine("\t(:requirements :adl :typing :universal-preconditions)");
                writer.WriteLine("\t(:types ");
                foreach (string type in domain.ObjectTypes)
                {
                    writer.Write("\t\t");
                    foreach (string subtype in domain.GetSubTypesOf(type))
                        writer.Write(subtype + " ");
                    writer.WriteLine("- " + type);
                }
                writer.WriteLine("\t)");
                writer.WriteLine("\t(:constants )");
                writer.WriteLine("\t(:predicates");
                foreach (Predicate pred in domain.Predicates)
                {
                    writer.Write("\t\t(" + pred.Name);
                    foreach (Term term in pred.Terms)
                    {
                        writer.Write(" " + term.Variable);
                        if (term.Type != "") writer.Write(" - " + term.Type);
                    }
                    writer.WriteLine(")");
                }
                writer.WriteLine("\t)");
                //writer.WriteLine(domain.staticStart);

                foreach (Operator action in domain.Operators)
                {
                    writer.WriteLine(Environment.NewLine + "\t(:action " + action.Name);
                    writer.Write("\t\t:parameters (");
                    foreach (Term term in action.Terms)
                    {
                        writer.Write(term.Variable + " ");
                        if (!term.Type.Equals(""))
                            writer.Write("- " + term.Type + " ");
                    }
                    writer.WriteLine(")");
                    writer.WriteLine("\t\t:precondition");
                    if (action.Preconditions.Count > 1)
                        writer.WriteLine("\t\t\t(and");
                    foreach (Predicate precon in action.Preconditions)
                        writer.WriteLine("\t\t\t\t" + precon.ToString());
                    if (action.Preconditions.Count > 1)
                        writer.WriteLine("\t\t\t)");
                    writer.WriteLine("\t\t:effect");
                    if (action.Effects.Count + action.Conditionals.Count > 1)
                        writer.WriteLine("\t\t\t(and");
                    foreach (Predicate effect in action.Effects)
                        writer.WriteLine("\t\t\t\t" + effect.ToString());
                    foreach (Axiom cond in action.Conditionals)
                    {
                        if (cond.Arity > 0)
                        {
                            writer.Write("\t\t\t\t(forall (");
                            foreach (ITerm term in cond.Terms)
                                writer.Write(term + " ");
                            writer.WriteLine(")");
                        }
                        writer.WriteLine("\t\t\t\t(when");
                        if (cond.Preconditions.Count > 1)
                            writer.WriteLine("\t\t\t\t(and");
                        foreach (Predicate precon in cond.Preconditions)
                            writer.WriteLine("\t\t\t\t\t" + precon.ToString());
                        if (cond.Preconditions.Count > 1)
                            writer.WriteLine("\t\t\t\t)");
                        if (cond.Effects.Count > 1)
                            writer.WriteLine("\t\t\t\t(and");
                        foreach (Predicate effect in cond.Effects)
                            writer.WriteLine("\t\t\t\t\t" + effect.ToString());
                        if (cond.Effects.Count > 1)
                            writer.WriteLine("\t\t\t\t)");
                        writer.WriteLine("\t\t\t\t)");
                        if (cond.Arity > 0)
                            writer.WriteLine("\t\t\t\t)");
                    }
                    if (action.Effects.Count + action.Conditionals.Count > 1)
                        writer.WriteLine("\t\t\t)");
                    writer.WriteLine("\t)");
                }

                writer.WriteLine(")");
            }
        }

        // Given a mediation node, creates an HTML representation.
        public static void ToHTML (string directory, PlanSpaceNode root)
        {
            string file = directory + root.id + ".html";

            using (StreamWriter writer = new StreamWriter(file, false))
            {
                writer.WriteLine("<html>");
                writer.WriteLine("<body>");
                if (root.incoming != null)
                {
                    //writer.WriteLine("Threatened Link: " + root.incoming.clobberedLink.Predicate.ToString(root.incoming.clobberedLink.Head.Bindings) + "<br />");
                    writer.WriteLine("Exceptional Step: " + root.incoming.action + "<br />");
                    writer.WriteLine("Establishing Step: " + root.incoming.clobberedLink.Tail + "<br />");
                    writer.WriteLine("Clobbered Step: " + root.incoming.clobberedLink.Head + "<br />");
                }    
                else
                {
                    writer.WriteLine("No threatened link.<br />");
                    writer.WriteLine("No exceptional step.<br />");
                    writer.WriteLine("No clobbered step.<br />");
                }
                StateToHTML(directory, root);
                writer.WriteLine("<a href='" + root.id + "state.html' target='_blank'>Initial State</a><br />");
                PlanToHTML(directory, root);
                writer.WriteLine("<a href='" + root.id + "plan.html' target='_blank'>Plan</a><br />");
                if (root.parent != null)
                {
                    writer.WriteLine("<br />Parent Node:<br />");
                    writer.WriteLine("<a href='" + root.parent.id + ".html'>Parent ID " + root.parent.id + "</a><br />");
                }
                if (root.children.Count > 0)
                {
                    writer.WriteLine("<br />Children Nodes:<br />");
                    foreach (PlanSpaceEdge action in root.outgoing)
                    {
                        PlanSpaceNode child = (PlanSpaceNode)root.children[action];
                        writer.WriteLine("<a href='" + child.id + ".html'>Child ID " + child.id + "</a><br />");
                    }
                }
                writer.WriteLine("</body>");
                writer.WriteLine("</html>");
            }

            foreach (PlanSpaceNode child in root.children.Values)
                ToHTML(directory, child);
        }

        // Given a mediation node, creates an HTML representation of its initial state.
        public static void StateToHTML(string directory, PlanSpaceNode root)
        {
            string file = directory + root.id + "state.html";

            using (StreamWriter writer = new StreamWriter(file, false))
            {
                writer.WriteLine("<html>");
                writer.WriteLine("<body>");
                foreach (Predicate pred in root.problem.Initial)
                    writer.WriteLine(pred + "<br />");
                writer.WriteLine("</body>");
                writer.WriteLine("</html>");
            }
        }

        // Given a mediation node, creates an HTML representation of its plan.
        public static void PlanToHTML(string directory, PlanSpaceNode root)
        {
            string file = directory + root.id + "plan.html";

            using (StreamWriter writer = new StreamWriter(file, false))
            {
                writer.WriteLine("<html>");
                writer.WriteLine("<body>");
                foreach (Operator action in root.plan.Steps)
                    if (!action.Name.Equals("initial") && !action.Name.Equals("goal"))
                        writer.WriteLine(action + "<br />");
                writer.WriteLine("</body>");
                writer.WriteLine("</html>");
            }
        }

        // Create a summary of the tree generated.
        public static void Summary (string directory, List<Tuple<String, String>> content)
        {
            string file = directory + "index.html";

            using (StreamWriter writer = new StreamWriter(file, false))
            {
                writer.WriteLine("<html>");
                writer.WriteLine("<body>");
                foreach (Tuple<String, String> tuple in content)
                {
                    writer.WriteLine("<b>" + tuple.First + "</b><br />");
                    writer.WriteLine(tuple.Second + "<br /><br />");
                }
                writer.WriteLine("<b>Root State</b><br />");
                writer.WriteLine("<a href='node-0.html'>Root</a><br />");
                writer.WriteLine("</body>");
                writer.WriteLine("</html>");
            }
        }

        // Given a mediation node, creates an HTML representation.
        public static void ToHTML (string directory, StateSpaceNode root)
        {
            string file = directory + "node-" + root.id + ".html";

            using (StreamWriter writer = new StreamWriter(file, false))
            {
                writer.WriteLine("<html>");
                writer.WriteLine("<body>");
                if (root.problem.Initial.Count > 0)
                    writer.WriteLine("<b>State</b><br />");
                foreach (Predicate pred in root.problem.Initial)
                    writer.WriteLine(pred + "<br />");
                if (root.problem.Initial.Count > 0)
                    writer.WriteLine("<br /><b>Observed State</b><br />");
                foreach (Predicate pred in root.problem.Initial)
                    if ((bool)pred.Observing(root.problem.Player))
                        writer.WriteLine(pred + "<br />");
                if (root.plan.Steps.Count > 0)
                    writer.WriteLine("<br /><b>Plan</b><br />");
                foreach (Operator action in root.plan.Steps)
                    writer.WriteLine(action + "<br />");
                if (root.children.Count > 0)
                {
                    writer.WriteLine("<br /><b>Player Actions</b><br />");
                    foreach (StateSpaceEdge action in root.outgoing)
                    {
                        StateSpaceNode child = (StateSpaceNode)root.children[action];
                        writer.WriteLine("<a href='node-" + child.id + ".html'>" + child.incoming.Action + "</a><br />");
                    }
                }
                if (root.parent != null)
                {
                    writer.WriteLine("<br /><b>Last State</b><br />");
                    writer.WriteLine("<a href='node-" + root.parent.id + ".html'>Parent ID " + root.parent.id + "</a><br />");
                }
                writer.WriteLine("</body>");
                writer.WriteLine("</html>");
            }

            foreach (StateSpaceNode child in root.children.Values)
                ToHTML(directory, child);
        }

        public static void ToHTML (string directory, GameTree.GameTree tree)
        {
            ToHTML(directory, tree.Root, tree);
            foreach (int key in tree.Tree.Keys)
                ToHTML(directory, tree.GetNode(key), tree);
        }

        // Given a game tree node, creates an HTML representation.
        public static void ToHTML(string directory, GameTreeNode root, GameTree.GameTree tree)
        {
            string file = directory + "node-" + root.ID + ".html";

            using (StreamWriter writer = new StreamWriter(file, false))
            {
                writer.WriteLine("<html>");
                writer.WriteLine("<body>");
                writer.WriteLine("<b>Plays</b><br />");
                writer.WriteLine(root.TimesPlayed + "<br /><br />");
                writer.WriteLine("<b>Wins</b><br />");
                writer.WriteLine(root.Wins + "<br /><br />");
                writer.WriteLine("<b>Interval</b><br />");
                writer.WriteLine(tree.GetInterval(root.ID) + "<br /><br />");
                if (root.State.Predicates.Count > 0)
                    writer.WriteLine("<b>State</b><br />");
                foreach (Predicate pred in root.State.Predicates)
                    writer.WriteLine(pred + "<br />");
                //if (root.Problem.Initial.Count > 0)
                //    writer.WriteLine("<br /><b>Observed State</b><br />");
                //foreach (Predicate pred in root.Problem.Initial)
                //    if ((bool)pred.Observing(root.Problem.Player))
                //        writer.WriteLine(pred + "<br />");
                if (root.Outgoing.Count > 0)
                {
                    writer.WriteLine("<br /><b>Enabled Actions</b><br />");
                    foreach (GameTreeEdge action in root.Outgoing)
                    {
                        if (action.Child != -1) writer.WriteLine("<a href='node-" + action.Child + ".html'>" + action.Action + "</a><br />");
                        else writer.WriteLine(action.Action + "<br />");
                    }
                }
                if (root.Incoming != null)
                    if (root.Incoming.Parent != -1)
                    {
                        writer.WriteLine("<br /><b>Last State</b><br />");
                        writer.WriteLine("<a href='node-" + root.Incoming.Parent + ".html'>Parent ID " + root.Incoming.Parent + "</a><br />");
                    }
                writer.WriteLine("</body>");
                writer.WriteLine("</html>");
            }
        }

        // Given a mediation node, creates an HTML representation of its initial state.
        public static void StateToHTML(string directory, StateSpaceNode root)
        {
            string file = directory + root.id + "state.html";

            using (StreamWriter writer = new StreamWriter(file, false))
            {
                writer.WriteLine("<html>");
                writer.WriteLine("<body>");
                foreach (Predicate pred in root.problem.Initial)
                    writer.WriteLine(pred + "<br />");
                writer.WriteLine("</body>");
                writer.WriteLine("</html>");
            }
        }

        // Given a mediation node, creates an HTML representation of its plan.
        public static void PlanToHTML(string directory, StateSpaceNode root)
        {
            string file = directory + root.id + "plan.html";

            using (StreamWriter writer = new StreamWriter(file, false))
            {
                writer.WriteLine("<html>");
                writer.WriteLine("<body>");
                foreach (Operator action in root.plan.Steps)
                    //if (!action.predicate.Equals("initial") && !action.predicate.Equals("goal"))
                        writer.WriteLine(action + "<br />");
                writer.WriteLine("</body>");
                writer.WriteLine("</html>");
            }
        }

        // Generates a CSV file of test statistics.
        public static void ToCSV (string directory, List<List<Tuple<String, String>>> summaries)
        {
            string file = directory + "summary.csv";

            using (StreamWriter writer = new StreamWriter(file, false))
            {
                List<Tuple<String, String>> firstSummary = summaries[0];
                Tuple<String, String> lastTuple = firstSummary[firstSummary.Count - 1];
                firstSummary.RemoveAt(firstSummary.Count - 1);
                foreach (Tuple<String, String> tuple in firstSummary)
                    writer.Write(tuple.First + ",");
                writer.WriteLine(lastTuple.First);
                foreach (Tuple<String, String> tuple in firstSummary)
                    writer.Write(tuple.Second + ",");
                writer.WriteLine(lastTuple.Second);

                summaries.RemoveAt(0);

                foreach (List<Tuple<String, String>> summary in summaries)
                {
                    lastTuple = summary[summary.Count - 1];
                    summary.RemoveAt(summary.Count - 1);
                    foreach (Tuple<String, String> tuple in summary)
                        writer.Write(tuple.Second + ",");
                    writer.WriteLine(lastTuple.Second);
                    summary.Add(lastTuple);
                }
            }
        }
    }
}
