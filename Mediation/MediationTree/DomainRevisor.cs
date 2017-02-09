using System.Collections.Generic;

using Mediation.PlanTools;
using Mediation.Utilities;
using Mediation.KnowledgeTools;
using Mediation.Interfaces;
using Mediation.Enums;
using Mediation.Planners;
using Mediation.FileIO;

namespace Mediation.MediationTree
{
    public static class DomainRevisor
    {
        public static void DomainRevision (Planner planner, MediationTreeNode node, MediationTree tree)
        {
            // Store the incoming action.
            Operator incoming = node.Incoming.Action as Operator;

            // Create a new plan object.
            Plan newPlan = new Plan();

            List<Tuple<Operator, State>> knowledge = EventRevisor.GetWorldKnowledge(tree.Player, node, tree);
            knowledge.RemoveAt(knowledge.Count - 1);
            List<Operator> observedActions = new List<Operator>();

            foreach (Tuple<Operator, State> pair in knowledge)
                if (pair.First != null)
                    if (pair.First.Name.Equals(incoming.Name)) observedActions.Add(pair.First);

            // Examine each exceptional effect.
            foreach (Predicate effect in incoming.ExceptionalEffects)
            {
                // Create a new domain object.
                Domain newDomain = node.Domain.Clone() as Domain;
                newDomain.Operators = new List<IOperator>();

                // If the current effect is conditional...
                if (incoming.IsConditional(effect))
                {
                    bool observed = false;

                    // If the conditional effect has not been observed by the player.
                    foreach (Operator action in observedActions)
                        if (!observed)
                            foreach (IAxiom conditional in action.Conditionals)
                                if (conditional.Effects.Contains(effect) && !observed) observed = true;

                    // Remove the conditional effect from the domain.

                    if (!observed)
                    {
                        // Copy the current operator templates into a list.
                        List<IOperator> templates = new List<IOperator>();
                        foreach (IOperator templ in node.Domain.Operators)
                            templates.Add(templ.Clone() as IOperator);

                        // Create a clone of the incoming action's unbound operator template.
                        IOperator temp = incoming.Template() as Operator;

                        // Find the incoming action template in the domain list.
                        Operator template = templates.Find(t => t.Equals(temp)) as Operator;

                        // Remove the incoming action template from the domain list.
                        templates.Remove(template);

                        // Create a clone of the incoming action.
                        Operator clone = incoming.Clone() as Operator;

                        // Create a list of conditional effects to remove from the template.
                        List<IAxiom> remove = new List<IAxiom>();
                        foreach (IAxiom conditional in clone.Conditionals)
                            if (conditional.Effects.Contains(effect))
                                remove.Add(conditional);

                        // Remove each conditional effect from the template.
                        foreach (IAxiom rem in remove)
                            template.Conditionals.Remove(rem.Template() as IAxiom);

                        // Add the modified template to the domain list.
                        templates.Add(template);

                        // Push the modified list to the new domain object.
                        newDomain.Operators = templates;

                        // Clone the modified incoming action template.
                        Operator newAction = template.Clone() as Operator;

                        // Bind the cloned action with the incoming action's bindings.
                        newAction.Bindings = incoming.Bindings;

                        MediationTreeEdge newEdge = new MediationTreeEdge(newAction, ActionType.Exceptional, node.Incoming.Parent, node.ID);

                        Problem newProblem = node.Problem.Clone() as Problem;
                        newProblem.Initial = tree.GetSuccessorState(newEdge).Predicates;

                        // Find a new plan.
                        newPlan = PlannerInterface.Plan(planner, newDomain, newProblem);

                        // If the modified domain can accommodate the player's action...
                        if (newPlan.Steps.Count > 0)
                        {
                            node.Plan = newPlan;
                            node.Incoming.Action = newAction;
                            node.State = tree.GetSuccessorState(node.Incoming);
                            node.Domain = newDomain;
                        }
                    }
                }
            }
        }
    }
}
