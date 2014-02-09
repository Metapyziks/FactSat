using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FactSat
{
    struct Literal
    {
        public readonly int Variable;
        public readonly bool Positive;

        public Literal(int var, bool pos)
        {
            Variable = var;
            Positive = pos;
        }

        public Literal(String str)
        {
            if (str[0] == '-') {
                Positive = false;
                Variable = int.Parse(str.Substring(1));
            } else {
                Positive = true;
                Variable = int.Parse(str);
            }
        }

        public override string ToString()
        {
 	         return Positive ? "" + Variable : "-" + Variable;
        }

        public override int GetHashCode()
        {
            return Positive ? Variable : ~Variable;
        }

        public override bool Equals(object obj)
        {
            return obj is Literal ? Equals((Literal) obj) : false;
        }

        public bool Equals(Literal literal)
        {
            return Variable == literal.Variable && Positive == literal.Positive;
        }
    }

    class Clause : IEnumerable<Literal>
    {
        private HashSet<Literal> _literals;

        public int Count
        {
            get { return _literals.Count; }
        }

        public Literal this[int index]
        {
            get { return _literals.ElementAt(index); }
        }

        public Clause()
        {
            _literals = new HashSet<Literal>();
        }

        public Clause(IEnumerable<Literal> literals)
        {
            _literals = new HashSet<Literal>(literals);
        }

        public Clause Clone()
        {
            return new Clause(_literals);
        }

        public void Add(Literal literal)
        {
            var old = _literals.FirstOrDefault(x => x.Variable == literal.Variable);
            if (old.Variable == literal.Variable) {
                if (old.Positive != literal.Positive) {
                    _literals.Remove(old);
                }

                return;
            }

            _literals.Add(literal);
        }

        public override string ToString()
        {
            return String.Format("{{{0}}}", String.Join(", ", _literals.Select(x => x.ToString())));
        }

        public override int GetHashCode()
        {
            return _literals.Aggregate(0, (a, x) => a ^ x.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            return obj is Clause ? Equals((Clause) obj) : false;
        }

        public bool Equals(Clause clause)
        {
            return _literals.Zip(clause._literals, (a, b) => a.Equals(b)).All(x => x);
        }

        public IEnumerator<Literal> GetEnumerator()
        {
            return _literals.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _literals.GetEnumerator();
        }
    }

    class Formula : IEnumerable<Clause>
    {
        public static Formula FromString(String str)
        {
            var clauseRegex = new Regex("^\\s*((-?[1-9][0-9]*)\\s+)+0\\s*$", RegexOptions.Multiline);
            var literalRegex = new Regex("-?[1-9][0-9]*");

            var formula = new Formula();
            var clauseMatch = clauseRegex.Match(str);
            while (clauseMatch.Success) {
                var clause = new Clause();
                var literalMatch = literalRegex.Match(clauseMatch.Value);
                while (literalMatch.Success) {
                    clause.Add(new Literal(literalMatch.Value));
                    literalMatch = literalMatch.NextMatch();
                }
                if (clause.Count > 0) formula.Add(clause);
                clauseMatch = clauseMatch.NextMatch();
            }

            return formula;
        }

        private HashSet<Clause> _clauses;
        private Literal _assignment;

        public int Count
        {
            get { return _clauses.Count; }
        }

        public bool Satisfied
        {
            get { return _clauses.Count == 0; }
        }

        public bool IsRoot { get { return Parent == null; } }

        public Formula Parent { get; private set; }

        public Formula this[Literal assignment]
        {
            get { return new Formula(this, assignment); }
        }

        public Formula this[int variable, bool assignment]
        {
            get { return new Formula(this, new Literal(variable, assignment)); }
        }
        
        private Formula()
        {
            Parent = null;

            _clauses = new HashSet<Clause>();
        }

        private Formula(Formula parent, Literal assignment)
            : this()
        {
            Parent = parent;
            _assignment = assignment;

            int variable = assignment.Variable;
            bool positive = assignment.Positive;

            foreach (var clause in parent._clauses) {
                if (!clause.Any(x => x.Variable == variable)) {
                    Add(clause.Clone());
                } else if (clause.Any(x => x.Variable == variable && x.Positive != positive)) {
                    Add(new Clause(clause.Where(x => x.Variable != variable)));
                }
            }
        }

        public void Add(Clause clause)
        {
            _clauses.Add(clause);
        }

        public void Add(params Literal[] clause)
        {
            _clauses.Add(new Clause(clause));
        }

        public IEnumerable<Literal> GetAssignments()
        {
            if (IsRoot) return Enumerable.Empty<Literal>();

            return Parent.GetAssignments().Union(new Literal[] { _assignment });
        }

        public Dictionary<int, bool> GetAssignmentsDict()
        {
            return GetAssignments().ToDictionary(x => x.Variable, x => x.Positive);
        }

        public override string ToString()
        {
            return String.Format("{{{0}}}", String.Join(", ", _clauses.Select(x => x.ToString())));
        }

        public IEnumerator<Clause> GetEnumerator()
        {
            return _clauses.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _clauses.GetEnumerator();
        }
    }
}
