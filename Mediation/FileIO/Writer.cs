using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

using Mediation.Interfaces;
using Mediation.PlanTools;
using Mediation.StateSpace;
using Mediation.PlanSpace;


namespace Mediation.FileIO
{
    public static class Writer
    {
        // Given a state, creates a PDDL file.
        public static void ToPDDL (string file, Domain domain, Problem problem, List<IPredicate> state)
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

        // Given a mediation node, creates an HTML representation.
        public static void ToHTML (string directory, StateSpaceNode root)
        {
            string file = directory + root.id + ".html";

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
                        writer.WriteLine("<a href='" + child.id + ".html'>" + child.incoming.Action + "</a><br />");
                    }
                }
                if (root.parent != null)
                {
                    writer.WriteLine("<br /><b>Last State</b><br />");
                    writer.WriteLine("<a href='" + root.parent.id + ".html'>Parent ID " + root.parent.id + "</a><br />");
                }
                writer.WriteLine("</body>");
                writer.WriteLine("</html>");
            }

            foreach (StateSpaceNode child in root.children.Values)
                ToHTML(directory, child);
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
    }
}
