using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FactSat
{
    class Program
    {
        static Formula UnitPropagation(Formula f)
        {
            Clause unitClause = null;

            while ((unitClause = f.FirstOrDefault(x => x.Count == 1)) != null) {
                f = f[unitClause[0]];
            }

            return f;
        }

        static Formula PureLiteral(Formula f)
        {
            bool eliminated;
            do {
                eliminated = false;
                var literals = f.SelectMany(x => x).Distinct().ToArray();
                var pure = literals.FirstOrDefault(x => !literals.Any(y => x.Variable == y.Variable));

                if (pure.Variable != 0) {
                    eliminated = true;
                    f = f[pure];
                }
            } while (eliminated);

            return f;
        }

        static Dictionary<int, int> _scores = new Dictionary<int, int>();

        static int PickSplitVariable(Formula f)
        {
            var assigned = f.GetAssignments();
            var valid = _scores.Keys
                .Where(x => !assigned.Any(y => y.Variable == x))
                .OrderByDescending(x => _scores[x]);

            int chosen = 0;
            if (valid.Count() == 0) {
                chosen = f
                    .SelectMany(x => x.Select(y => y.Variable))
                    .GroupBy(x => x)
                    .Select(x => Tuple.Create(x.Key, x.Count()))
                    .OrderByDescending(x => x.Item2)
                    .First().Item1;

                _scores.Add(chosen, 0);
            } else {
                chosen = valid.First();
                ++_scores[chosen];
            }

            return chosen;
        }

        static Formula Satisfy(Formula f)
        {
            f = UnitPropagation(f);
            f = PureLiteral(f);

            if (f.Count == 0) return f;
            if (f.Any(x => x.Count == 0)) return null;

            var assign = f.GetAssignments();
            Console.WriteLine("{0}%", (assign.Count() * 100) / 1562);

            var split = PickSplitVariable(f);

            return Satisfy(f[split, false]) ?? Satisfy(f[split, true]);
        }

        static BigInteger IntFromBits(Formula f, IEnumerable<int> variables)
        {
            BigInteger num = 0;

            var dict = f.GetAssignmentsDict();

            foreach (var variable in variables) {
                num <<= 1;
                num |= (dict[variable] ? 1 : 0);
            }

            return num;
        }

        static void Main(string[] args)
        {
            var instance = Factorisation.FromString(File.ReadAllText(args[0]));
            var f = Satisfy(instance.RootFormula);

            if (f.Satisfied) {
                Console.WriteLine("Satisfied!");

                var inpA = IntFromBits(f, instance.Input1Bits);
                var inpB = IntFromBits(f, instance.Input2Bits);
                var outp = IntFromBits(f, instance.OutputBits);

                Console.WriteLine("{0} x {1} = {2}", inpA, inpB, outp);
            } else {
                Console.WriteLine("Could not satisfy :(");
            }

            Console.ReadKey();
        }
    }
}
