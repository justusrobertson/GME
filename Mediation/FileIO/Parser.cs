#define DEBUG
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;

using Mediation.Interfaces;
using Mediation.PlanTools;

using Mediation.Enums;

namespace Mediation.FileIO
{
    public static class Parser
    {
        public static string path = @"C:\MediationService\";

        // Returns the project's top directory as a string.
        public static string GetTopDirectory ()
        {
            // Split the current directory path by \.
            string[] splPath = Directory.GetCurrentDirectory().Split('\\');

            // Create a string to hold the directory path.
            string topDir = "";

            // Loop through the split path.
            for (int i = 0; i < splPath.Length - 3; i++)
                // Add the first n - 2 directories from the path.
                topDir += splPath[i] + '\\';

            // Return the new path string.
            //return @"C:\MediationService\";
            //return @"J:\Code\Mediation\GME\";
            #if (DEBUG)
                path = @"J:\Code\Mediation\GME\";
            #endif

            if (path.Equals("")) return topDir;
            else return path;
        }

        // Returns the project's top directory for a sh script.
        public static string GetScriptDirectory ()
        {
            // Remove the drive colon from the top directory's path.
            string shDir = Regex.Replace(GetTopDirectory(), ":", "");

            // Replace all backslashes with forwardslashes.
            shDir = Regex.Replace(shDir, @"\\", "/");

            // Remove leading and trailing slashes.
            shDir = shDir.Trim('/');

            // Return the computed string.
            return shDir;
        }

        // Reads in a plan from a file.
        public static Plan GetPlan (string file, Domain domain, Problem problem)
        {
            // The plan object.
            Plan plan = new Plan();

            // Add the domain to the plan.
            plan.Domain = domain;

            // Add the problem to the plan.
            plan.Problem = problem;

            // Create a step to represent the initial state.
            Operator init = new Operator();

            // Name the initial step.
            init.Name = "initial";

            // Set the step effects to the initial state.
            init.Effects = problem.Initial;

            // Add the step to the plan.
            plan.InitialStep = init;

            // Read the file into an array.
            string[] input = System.IO.File.ReadAllLines(file);

            // How to split the file elements.
            char[] delimiterChars = { ' ', '(', ')' };

            // Fail gracefully.
            if (input.Length > 0)
                // Check for Glaive input.
                if (input[0].Split(delimiterChars)[1].Equals("define") && input[0].Split(delimiterChars)[3].Equals("plan"))
                {
                    // Create a new array.
                    string[] glaiveOutputIsDumb = new string[input.Length - 2];

                    // Edit the first line.
                    glaiveOutputIsDumb[0] = input[2].Substring(10);

                    // Get rid of the first two lines.
                    for (int i = 3; i < input.Length; i++)
                        glaiveOutputIsDumb[i - 2] = input[i].Trim();

                    // Set the corrected output to the input array.
                    input = glaiveOutputIsDumb;
                }

            // Iterate through the plain text plan.
            foreach (string line in input)
            {
                // Split line into elements.
                string[] words = line.Split(delimiterChars);

                // Create an empty action template for the domain template.
                IOperator temp = null;

                // If this is a plan space plan...
                if (domain.Type == PlanType.PlanSpace)
                {
                    // Create the step.
                    Operator step = new Operator();

                    // Set the predicate to the first element.
                    step.Name = words[1];

                    // Find the corresponding operator in the domain.
                    temp = (Operator)domain.Operators.Find(x => x.Name.Equals(step.Name));

                    // Create a counter for the loop.
                    int i = 0;

                    // Set the terms to the proceeding elements.
                    foreach (string word in words.Skip(2))
                    {
                        if (!word.Equals(""))
                        {
                            // Add the variable name for the current term.
                            step.Terms.Add(temp.Predicate.TermAt(i));

                            // Hash the variable name to the binding.
                            //step.Bindings.Add(temp.Predicate.TermAt(i), word);

                            // Iterate the counter.
                            i++;
                        }
                    }

                    // Copy the operator's preconditions.
                    List<IPredicate> preconditions = new List<IPredicate>();
                    foreach (IPredicate precon in temp.Preconditions)
                        preconditions.Add((Predicate)precon.Clone());

                    // Copy the operator's preconditions.
                    step.Preconditions = preconditions;

                    // Copy the operator's effects.
                    List<IPredicate> effects = new List<IPredicate>();
                    foreach (IPredicate effect in temp.Effects)
                        effects.Add((Predicate)effect.Clone());

                    // Copy the operator's effects.
                    step.Effects = effects;

                    // Add the current step to the plan.
                    plan.Steps.Add(step);
                }
                // Otherwise, if this is a state space plan...
                else if (domain.Type == PlanType.StateSpace)
                {
                    // Create the step.
                    Operator step = new Operator();

                    // Set the predicate to the first element.
                    step.Name = words[1];

                    // Find the corresponding operator in the domain.
                    temp = (Operator)domain.Operators.Find(x => x.Name.Equals(step.Name));

                    // Create a counter for the loop.
                    int i = 0;

                    // Set the terms to the proceeding elements.
                    foreach (string word in words.Skip(2))
                    {
                        if (!word.Equals(""))
                        {
                            // Add the variable name for the current term.
                            step.Terms.Add(new Term(temp.Predicate.TermAt(i).Variable, word, temp.Predicate.TermAt(i).Type));

                            // Iterate the counter.
                            i++;
                        }
                    }

                    // Copy the operator's preconditions.
                    List<IPredicate> preconditions = new List<IPredicate>();
                    foreach (IPredicate precon in temp.Preconditions)
                        preconditions.Add(precon.Clone() as Predicate);

                    // Copy the operator's preconditions.
                    step.Preconditions = preconditions;

                    // Copy the operator's effects.
                    List<IPredicate> effects = new List<IPredicate>();
                    foreach (IPredicate effect in temp.Effects)
                        effects.Add(effect.Clone() as Predicate);

                    // Copy the operator's effects.
                    step.Effects = effects;

                    // Copy the operator's axioms.
                    List<IAxiom> conditionals = new List<IAxiom>();
                    foreach (IAxiom conditional in temp.Conditionals)
                        conditionals.Add(conditional.Clone() as Axiom);

                    // Copy the operator's axioms.
                    step.Conditionals = conditionals;

                    // Add the current step to the plan.
                    plan.Steps.Add(step);
                }
            }

            // Create a step to represent the goal state.
            Operator goal = new Operator();

            // Name the goal step.
            goal.Name = "goal";

            // Set the step preconditions to the goal state.
            goal.Preconditions = problem.Goal;

            // Add the step to the plan.
            plan.GoalStep = goal;

            // Overwrite file.
            using (StreamWriter writer = new StreamWriter(file, false))
            {
                writer.Write("");
            }

            return plan;
        }

        // Reads in a domain from a file.
        public static Domain GetDomain (string file, PlanType type)
        {
            // The domain object.
            Domain domain = new Domain();

            // Set the domain's type.
            domain.Type = type;

            // Read the domain file into a string.
            string input = System.IO.File.ReadAllText(file);

            // Split the input string by space, line feed, character return, and tab.
            string[] words = input.Split(new char[] {' ', '\r', '\n', '\t'});

            // Remove all empty elements of the word array.
            words = words.Where(x => !string.IsNullOrEmpty(x)).ToArray();

            // Loop through the word array.
            for (int i = 0; i < words.Length; i++)
            {
                // Set the domain name.
                if (words[i].Equals("(domain"))
                    domain.Name = words[i + 1].Remove(words[i + 1].Length - 1);

                // Begin types definitions.
                if (words[i].Equals("(:types"))
                {
                    // If the list is not empty.
                    if (!words[i + 1].Equals(")"))
                        // Loop until list is finished.
                        while (words[i][words[i].Length - 1] != ')')
                        {
                            // Create a list for sub-types.
                            List<string> subTypes = new List<string>();

                            // Read in the sub-types.
                            while (!Regex.Replace(words[++i], @"\t|\n|\r", "").Equals("-"))
                                subTypes.Add(Regex.Replace(words[i], @"\t|\n|\r", ""));

                            // Associate sub-types with type in domain object.
                            domain.AddTypeList(subTypes, Regex.Replace(words[++i], @"\t|\n|\r|[()]", ""));
                        }
                }

                // Begin constants definitions.
                if (words[i].Equals("(:constants"))
                {
                    // If the list is not empty.
                    if (!words[i + 1].Equals(")"))
                        // Loop until list is finished.
                        while (words[i][words[i].Length - 1] != ')')
                        {
                            // Create a list for sub-types.
                            List<string> constants = new List<string>();

                            // Read in the sub-types.
                            while (!Regex.Replace(words[++i], @"\t|\n|\r", "").Equals("-"))
                                constants.Add(Regex.Replace(words[i], @"\t|\n|\r", ""));

                            // Associate sub-types with type in domain object.
                            domain.AddConstantsList(constants, Regex.Replace(words[++i], @"\t|\n|\r|[()]", ""));
                        }
                }

                // Begin an action definition.
                if (words[i].Equals("(:action"))
                {
                    IOperator temp = null;

                    if (type == PlanType.PlanSpace)
                        // Create an operator object.
                        temp = new Operator();
                    else if (type == PlanType.StateSpace)
                        // Create an action object.
                        temp = new Operator();

                    // Name the operator's predicate.
                    temp.Name = Regex.Replace(words[i + 1], @"\t|\n|\r", "");

                    // Add the operator to the domain object.
                    domain.Operators.Add(temp);
                }

                // Fill in an operator's internal information.
                if (words[i].Equals(":parameters"))
                {
                    // Add the operator's parameters.
                    while (!Regex.Replace(words[i++], @"\t|\n|\r", "").Equals(":precondition"))
                        if (words[i][0] == '(' || words[i][0] == '?')
                        {
                            // Create a new term using the variable name.
                            Term term = new Term(Regex.Replace(words[i], @"\t|\n|\r|[()]", ""));
                            
                            // Check if the term has a specified type.
                            if (Regex.Replace(words[i + 1], @"\t|\n|\r", "").Equals("-"))
                            {
                                // Iterate the counter past the dash.
                                i++;

                                // Add the type to the term object.
                                term.Type = Regex.Replace(words[++i], @"\t|\n|\r|[()]", "");
                            }

                            // Add the term to the operator's predicate.
                            domain.Operators.Last().Predicate.Terms.Add(term);
                        }

                    // Create a list to hold the preconditions.
                    List<IPredicate> preconditions = new List<IPredicate>();

                    // Add the operator's preconditions.
                    while (!Regex.Replace(words[i++], @"\t|\n|\r", "").Equals(":effect"))
                    {
                        if (words[i][0] == '(')
                        {
                            if(!words[i].Equals("(and"))
                            {
                                // Create a new precondition object.
                                Predicate pred = new Predicate();

                                // Check for a negative precondition.
                                if (words[i].Equals("(not"))
                                {
                                    // Iterate the counter.
                                    i++;

                                    // Set the effect's sign to false.
                                    pred.Sign = false;
                                }

                                // Set the precondition's name.
                                pred.Name = Regex.Replace(words[i], @"\t|\n|\r|[()]", "");

                                // Add the precondition to the operator.
                                preconditions.Add(pred);
                            }
                        }
                        else
                        {
                            // Add the precondition's terms.
                            if (!Regex.Replace(words[i], @"\t|\n|\r", "").Equals(":effect") && !words[i].Equals(")"))
                                if (Regex.Replace(words[i], @"\t|\n|\r|[()]", "")[0] == '?')
                                    preconditions.Last().Terms.Add(new Term(Regex.Replace(words[i], @"\t|\n|\r|[()]", "")));
                                else
                                    preconditions.Last().Terms.Add(new Term(Regex.Replace(words[i], @"\t|\n|\r|[()]", ""), true));
                        }
                    }

                    // Add the preconditions to the last created operator.
                    domain.Operators.Last().Preconditions = preconditions;

                    // Create a list to hold the effects.
                    List<IPredicate> effects = new List<IPredicate>();

                    // Add the operator's effects.
                    while (!Regex.Replace(words[i + 1], @"\t|\n|\r", "").Equals("(:action") && !Regex.Replace(words[i], @"\t|\n|\r", "").Equals(":agents") && i < words.Length - 2)
                    {
                        if (words[i][0] == '(')
                        {
                            // Check for a conditional effect.
                            // THIS SHOULD PROBABLY BE CONDENSED
                            if (words[i].Equals("(forall"))
                            {
                                // Create a new axiom object.
                                Axiom axiom = new Axiom();
                                
                                // Read in the axiom's terms.
                                while (!Regex.Replace(words[++i], @"\t|\n|\r", "").Equals("(when"))
                                    axiom.Terms.Add(new Term(Regex.Replace(words[i], @"\t|\n|\r|[()]", "")));

                                // If the preconditions are conjunctive.
                                if (Regex.Replace(words[++i], @"\t|\n|\r", "").Equals("(and"))
                                {
                                    // Initialize a parentheses stack counter.
                                    int parenStack = 1;
                                    i++;

                                    // Use the stack to loop through the conjunction.
                                    while (parenStack > 0)
                                    {
                                        // Check for an open paren.
                                        if (words[i][0] == '(')
                                        {
                                            // Create new predicate.
                                            Predicate pred = new Predicate();

                                            // Check for a negative effect.
                                            if (words[i].Equals("(not"))
                                            {
                                                // Iterate the counter.
                                                i++;

                                                // Set the effect's sign to false.
                                                pred.Sign = false;
                                            }

                                            // Name the predicate.
                                            pred.Name = Regex.Replace(words[i++], @"\t|\n|\r|[()]", "");

                                            // Read in the terms.
                                            while (words[i][words[i].Length - 1] != ')')
                                                pred.Terms.Add(new Term(Regex.Replace(words[i++], @"\t|\n|\r|[()]", "")));

                                            // Read the last term.
                                            pred.Terms.Add(new Term(Regex.Replace(words[i++], @"\t|\n|\r|[()]", "")));

                                            // Add the predicate to the axiom's preconditions.
                                            axiom.Preconditions.Add(pred);
                                        }

                                        // Check for a close paren.
                                        if (words[i][words[i].Length - 1] == ')')
                                            parenStack--;
                                    }
                                }
                                else
                                {
                                    // Check for an open paren.
                                    if (words[i][0] == '(')
                                    {
                                        // Create new predicate.
                                        Predicate pred = new Predicate();

                                        // Check for a negative effect.
                                        if (words[i].Equals("(not"))
                                        {
                                            // Iterate the counter.
                                            i++;

                                            // Set the effect's sign to false.
                                            pred.Sign = false;
                                        }

                                        // Name the predicate.
                                        pred.Name = Regex.Replace(words[i++], @"\t|\n|\r|[()]", "");

                                        // Read in the terms.
                                        while (words[i][words[i].Length - 1] != ')')
                                            pred.Terms.Add(new Term(Regex.Replace(words[i++], @"\t|\n|\r|[()]", "")));

                                        // Read the last term.
                                        pred.Terms.Add(new Term(Regex.Replace(words[i++], @"\t|\n|\r|[()]", "")));

                                        // Add the predicate to the axiom's preconditions.
                                        axiom.Preconditions.Add(pred);
                                    }
                                }

                                // If the preconditions are conjunctive.
                                if (Regex.Replace(words[++i], @"\t|\n|\r", "").Equals("(and"))
                                {
                                    // Initialize a parentheses stack counter.
                                    int parenStack = 1;
                                    i++;

                                    // Use the stack to loop through the conjunction.
                                    while (parenStack > 0)
                                    {
                                        // Check for an open paren.
                                        if (words[i][0] == '(')
                                        {
                                            // Create new predicate.
                                            Predicate pred = new Predicate();

                                            // Check for a negative effect.
                                            if (words[i].Equals("(not"))
                                            {
                                                // Iterate the counter.
                                                i++;

                                                // Set the effect's sign to false.
                                                pred.Sign = false;
                                            }

                                            // Name the predicate.
                                            pred.Name = Regex.Replace(words[i++], @"\t|\n|\r|[()]", "");

                                            // Read in the terms.
                                            while (words[i][words[i].Length - 1] != ')')
                                                pred.Terms.Add(new Term(Regex.Replace(words[i++], @"\t|\n|\r|[()]", "")));

                                            // Read the last term.
                                            pred.Terms.Add(new Term(Regex.Replace(words[i++], @"\t|\n|\r|[()]", "")));

                                            // Add the predicate to the axiom's effects.
                                            axiom.Effects.Add(pred);
                                        }

                                        // Check for a close paren.
                                        if (words[i][words[i].Length - 1] == ')')
                                            parenStack--;
                                    }
                                }
                                else
                                {
                                    // Check for an open paren.
                                    if (words[i][0] == '(')
                                    {
                                        // Create new predicate.
                                        Predicate pred = new Predicate();

                                        // Check for a negative effect.
                                        if (words[i].Equals("(not"))
                                        {
                                            // Iterate the counter.
                                            i++;

                                            // Set the effect's sign to false.
                                            pred.Sign = false;
                                        }

                                        // Name the predicate.
                                        pred.Name = Regex.Replace(words[i++], @"\t|\n|\r|[()]", "");

                                        // Read in the terms.
                                        while (words[i][words[i].Length - 1] != ')')
                                            pred.Terms.Add(new Term(Regex.Replace(words[i++], @"\t|\n|\r|[()]", "")));

                                        // Read the last term.
                                        pred.Terms.Add(new Term(Regex.Replace(words[i++], @"\t|\n|\r|[()]", "")));

                                        // Add the predicate to the axiom's effects.
                                        axiom.Effects.Add(pred);
                                    }
                                }

                                // Add the axiom to the set of conditional effects.
                                domain.Operators.Last().Conditionals.Add(axiom);
                            }
                            else if (!words[i].Equals("(and"))
                            {
                                // Create a new effect object.
                                Predicate pred = new Predicate();

                                // Check for a negative effect.
                                if (words[i].Equals("(not"))
                                {
                                    // Iterate the counter.
                                    i++;

                                    // Set the effect's sign to false.
                                    pred.Sign = false;
                                }

                                // Set the effect's name.
                                pred.Name = Regex.Replace(words[i], @"\t|\n|\r|[()]", "");

                                // Add the effect to the operator.
                                effects.Add(pred);
                            }
                        }
                        else
                        {
                            // Add the effect's terms.
                            if (!Regex.Replace(words[i], @"\t|\n|\r", "").Equals("(:action") && !words[i].Equals(")"))
                                if (Regex.Replace(words[i], @"\t|\n|\r|[()]", "")[0] == '?')
                                    effects.Last().Terms.Add(new Term(Regex.Replace(words[i], @"\t|\n|\r|[()]", "")));
                                else
                                    effects.Last().Terms.Add(new Term(Regex.Replace(words[i], @"\t|\n|\r|[()]", ""), true));
                        }

                        // Iterate the counter.
                        i++;
                    }

                    // Add the effects to the last created operator.
                    domain.Operators.Last().Effects = effects;

                    // Create a list for storing consenting agents.
                    List<ITerm> consenting = new List<ITerm>();

                    // Check if the action has any consenting agents.
                    if (Regex.Replace(words[i], @"\t|\n|\r", "").Equals(":agents"))
                    {
                        // If so, iterate through them.
                        while (Regex.Replace(words[++i], @"\t|\n|\r", "")[Regex.Replace(words[i], @"\t|\n|\r", "").Length - 1] != ')')
                            // And add them to the list.
                            consenting.Add(new Term(Regex.Replace(words[i], @"\t|\n|\r|[()]", "")));

                        // Add the final item to the list.
                        consenting.Add(new Term(Regex.Replace(words[i], @"\t|\n|\r|[()]", "")));
                    }

                    // Add the consenting agents to the action.
                    domain.Operators.Last().ConsentingAgents = consenting;
                }
            }

            return domain;
        }

        // Reads in a problem from a file.
        public static Problem GetProblem (string file)
        {
            // Create the problem object.
            Problem problem = new Problem();

            // Read the domain file into a string.
            string input = System.IO.File.ReadAllText(file);

            // Split the input string by space, line feed, character return, and tab.
            string[] words = input.Split(new char[] { ' ', '\r', '\n', '\t' });

            // Remove all empty elements of the word array.
            words = words.Where(x => !string.IsNullOrEmpty(x)).ToArray();

            // Loop through the word array.
            for (int i = 0; i < words.Length; i++)
            {
                // Set the problem name.
                if (words[i].Equals("(problem"))
                    problem.Name = words[i + 1].Remove(words[i + 1].Length - 1);

                // Set the domain name.
                if (words[i].Equals("(:domain"))
                    problem.Domain = words[i + 1].Remove(words[i + 1].Length - 1);

                // Fill in the problem's internal information.
                if (words[i].Equals("(:objects"))
                {
                    // A list of temporary objects to store before we know their type.
                    List<string> tempObjects = new List<string>();

                    // Add the problem objects.
                    while (!Regex.Replace(words[i++], @"\t|\n|\r", "").ToLower().Equals("(:init"))
                        if (!Regex.Replace(words[i], @"\t|\n|\r", "").ToLower().Equals("(:init"))
                            if (!Regex.Replace(words[i], @"\t|\n|\r", "").ToLower().Equals("-"))
                                tempObjects.Add(Regex.Replace(words[i], @"\t|\n|\r|[()]", "").ToLower());
                            else
                            { 
                                // Store the specified type.
                                string type = Regex.Replace(words[++i], @"\t|\n|\r|[()]", "").ToLower();

                                // For all the stored objects...
                                foreach (string tempObj in tempObjects)
                                    // ... associate them with their type and add them to the problem.
                                    problem.Objects.Add(new Obj(tempObj, type));

                                // Clear the temporary objects list.
                                tempObjects = new List<string>();
                            }

                    // Add objects with unspecified types to the problem.
                    foreach (string tempObj in tempObjects)
                        problem.Objects.Add(new Obj(tempObj, ""));

                    // Add the initial state.
                    while (!Regex.Replace(words[i], @"\t|\n|\r", "").ToLower().Equals("(:goal"))
                    {
                        if (words[i][0] == '(')
                        {
                            // Check for an intention predicate.
                            if (words[i].Equals("(intends"))
                            {
                                // Create a new intention object.
                                Intention intends = new Intention();

                                // Add the character to the intention object.
                                intends.Character = words[++i];

                                // Check for a negative predicate.
                                if (words[++i].Equals("(not"))
                                {
                                    // Iterate the counter.
                                    i++;

                                    // Set the predicate's sign to false.
                                    intends.Predicate.Sign = false;
                                }

                                // Set the predicate's name.
                                intends.Predicate.Name = Regex.Replace(words[i++], @"\t|\n|\r|[()]", "");

                                // Add the predicates's terms.
                                while (words[i][0] != '(')
                                    intends.Predicate.Terms.Add(new Term("", Regex.Replace(words[i++], @"\t|\n|\r|[()]", "")));

                                // Add the intention to the problem.
                                problem.Intentions.Add(intends.Clone() as Intention);
                            }
                            else
                            {
                                // Create a new predicate object.
                                Predicate pred = new Predicate();

                                // Check for a negative predicate.
                                if (words[i].Equals("(not"))
                                {
                                    // Iterate the counter.
                                    i++;

                                    // Set the predicate's sign to false.
                                    pred.Sign = false;
                                }

                                // Set the predicate's name.
                                pred.Name = Regex.Replace(words[i++], @"\t|\n|\r|[()]", "");

                                // Add the predicates's terms.
                                while (words[i][0] != '(')
                                    if (!Regex.Replace(words[i], @"\t|\n|\r|[()]", "").Equals(""))
                                        pred.Terms.Add(new Term("", Regex.Replace(words[i++], @"\t|\n|\r|[()]", "")));
                                    else
                                        i++;

                                // Add the predicate to the initial state.
                                problem.Initial.Add(pred);
                            }
                        }
                    }

                    // Add the goal state.
                    while (i++ < words.Length - 1)
                    {
                        if (words[i][0] == '(')
                        {
                            if (!words[i].ToLower().Equals("(and"))
                            {
                                // Create a new predicate object.
                                Predicate pred = new Predicate();

                                // Check for a negative predicate.
                                if (words[i].Equals("(not"))
                                {
                                    // Iterate the counter.
                                    i++;

                                    // Set the predicate's sign to false.
                                    pred.Sign = false;
                                }

                                // Set the predicate's name.
                                pred.Name = Regex.Replace(words[i], @"\t|\n|\r|[()]", "");

                                // Add the predicate to the goal state.
                                problem.Goal.Add(pred);
                            }
                        }
                        else
                        {
                            // Add the predicate's terms.
                            if (!words[i].Equals(")"))
                                problem.Goal.Last().Terms.Add(new Term("", Regex.Replace(words[i], @"\t|\n|\r|[()]", "")));
                        }
                    }
                }
            }

            // Kind of a hack.
            problem.OriginalName = problem.Name;

            return problem;
        }

        // Reads in a problem and fills in its object types.
        public static Problem GetProblemWithTypes (string file, Domain domain)
        {
            // Read the problem file into an object.
            Problem problem = GetProblem (file);

            // Add type associations to each object.
            foreach (string type in domain.ObjectTypes)
                foreach (string subtype in domain.GetSubTypesOf(type))
                    foreach (IObject obj in problem.Objects)
                        if (obj.SubType.Equals(subtype))
                            obj.Types.Add(type);

            // Return the problem object.
            return problem;
        }
    }
}