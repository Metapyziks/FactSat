using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FactSat
{
    class Factorisation
    {
        public static Factorisation FromProblemString(string str)
        {
            var factor = new Factorisation();

            factor.RootFormula = Formula.FromString(str);

            var regexFormat = "^c\\s+[a-zA-Z ]+{0}[^:]*:\\s*\\[(?<locations>[0-9, ]+)\\]\\s*$";

            var outputRegex = new Regex(String.Format(regexFormat, "output"), RegexOptions.Multiline);
            var input1Regex = new Regex(String.Format(regexFormat, "first input"), RegexOptions.Multiline);
            var input2Regex = new Regex(String.Format(regexFormat, "second input"), RegexOptions.Multiline);

            var match = outputRegex.Match(str);
            if (!match.Success) throw new FormatException("Given CNF file is formatted incorrectly.");

            factor.OutputBits = match.Groups["locations"].Value.Split(',')
                .Select(x => int.Parse(x.Trim())).ToArray();

            match = input1Regex.Match(str);
            if (!match.Success) throw new FormatException("Given CNF file is formatted incorrectly.");

            factor.Input1Bits = match.Groups["locations"].Value.Split(',')
                .Select(x => int.Parse(x.Trim())).ToArray();

            match = input2Regex.Match(str);
            if (!match.Success) throw new FormatException("Given CNF file is formatted incorrectly.");

            factor.Input2Bits = match.Groups["locations"].Value.Split(',')
                .Select(x => int.Parse(x.Trim())).ToArray();

            return factor;
        }

        public Formula RootFormula { get; private set; }
        public Dictionary<int, bool> Solution { get; private set; }
        public IEnumerable<int> OutputBits { get; private set; }
        public IEnumerable<int> Input1Bits { get; private set; }
        public IEnumerable<int> Input2Bits { get; private set; }

        private Factorisation() { }

        public void ReadSolutionFromString(String str)
        {
            if (!str.StartsWith("SAT")) {
                Solution = null;
                return;
            }

            var regex = new Regex("-?[1-9][0-9]*");

            Solution = new Dictionary<int, bool>();

            var match = regex.Match(str);
            while (match.Success) {
                int assign = int.Parse(match.Value);

                if (assign < 0) {
                    Solution.Add(-assign, false);
                } else {
                    Solution.Add(assign, true);
                }

                match = match.NextMatch();
            }
        }
    }
}
