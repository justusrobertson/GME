using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Mediation.Interfaces;

namespace Mediation.PlanTools
{
    [Serializable]
    public class CausalLink : IDependency
    {
        private Predicate predicate;
        private Operator head;
        private Operator tail;
        private List<IOperator> span;

        // Access the link's predicate.
        public Predicate Predicate
        {
            get { return predicate; }
            set { predicate = value; }
        }

        // Access the link's head.
        public IOperator Head
        {
            get { return head; }
            set { head = (Operator)value; }
        }

        // Access the link's tail.
        public IOperator Tail
        {
            get { return tail; }
            set { tail = (Operator)value; }
        }

        // Access the link's span.
        public List<IOperator> Span
        {
            get { return span; }
            set { span = value; }
        }

        public CausalLink ()
        {
            predicate = new Predicate();
            head = new Operator();
            tail = new Operator();
            span = new List<IOperator>();
        }

        public CausalLink (Predicate predicate, Operator head, Operator tail)
        {
            this.predicate = predicate;
            this.head = head;
            this.tail = tail;
            this.span = new List<IOperator>();
        }

        public CausalLink(Predicate predicate, Operator head, Operator tail, List<IOperator> span)
        {
            this.predicate = predicate;
            this.head = head;
            this.tail = tail;
            this.span = span;
        }

        // Returns a bound copy of the predicate.
        public Predicate GetBoundPredicate ()
        {
            // Clone the predicate.
            Predicate boundPred = (Predicate)predicate.Clone();

            // Bind the predicate to the tail's bindings.
            /*if (tail.Bindings.Keys.Count > 0)
                boundPred.BindTerms(tail.Bindings);
            else
                boundPred.BindTerms(head.Bindings);*/

            // Return the bound predicate.
            return boundPred;
        }

        // Displays the contents of the causal link.
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Predicate: " + predicate);

            sb.AppendLine("Tail: " + tail.ToString());
            
            sb.AppendLine("Head: " + head.ToString());

            return sb.ToString();
        }

        // Create a clone of the causal link.
        public Object Clone ()
        {
            List<IOperator> newSpan = new List<IOperator>();
            foreach (Operator op in Span)
                newSpan.Add(op.Clone() as Operator);

            return new CausalLink((Predicate)predicate.Clone(), (Operator)head.Clone(), (Operator)tail.Clone(), newSpan);
        }
    }
}
