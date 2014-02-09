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

            while (!f.Contradiction && (unitClause = f.FirstOrDefault(x => x.Count == 1)) != null) {
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

        static int PickSplitVariable(Formula f)
        {
            var vars = f
                .SelectMany(x => x)
                .Select(x => x.Variable)
                .Distinct()
                .ToArray();

            var minLength = f
                .Where(x => x.Count > 0)
                .Min(x => x.Count);

            var shortest = f
                .Where(x => x.Count == minLength)
                .ToArray();

            Dictionary<Literal, int> occurances = new Dictionary<Literal, int>(vars.Length * 2);

            foreach (var var in vars) {
                occurances.Add(new Literal(var, false), 0);
                occurances.Add(new Literal(var, true), 0);
            }

            foreach (var clause in shortest) {
                foreach (var literal in clause) {
                    ++occurances[literal];
                }
            }

            var choice = vars
                .Select(x => {
                    int n = occurances[new Literal(x, false)];
                    int p = occurances[new Literal(x, true)];
                    return Tuple.Create(x, (1 << 16) * (n + p) + n * p);
                })
                .OrderByDescending(x => x.Item2)
                .First().Item1;

            return choice;
        }

        static Formula Satisfy(Formula f)
        {
            f = UnitPropagation(f);
            // f = PureLiteral(f);

            if (f.Contradiction) return null;
            if (f.Count == 0) return f;

            var assign = f.GetAssignments().ToArray();
            Console.WriteLine("{0}%", (assign.Count() * 100) / (f.SelectMany(x => x).Select(x => x.Variable).Distinct().Count() + assign.Count()));

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

            if (f != null) {
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
