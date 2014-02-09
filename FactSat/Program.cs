using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        static int PickSplitVariable(Formula f)
        {
            return f.First().First().Variable;
        }

        static bool Satisfiable(Formula f)
        {
            f = UnitPropagation(f);
            f = PureLiteral(f);

            if (f.Count == 0) return true;
            if (f.Any(x => x.Count == 0)) return false;

            var split = PickSplitVariable(f);

            return Satisfiable(f[split, false]) || Satisfiable(f[split, true]);
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Satisfiable: {0}", Satisfiable(Formula.FromString(File.ReadAllText(args[0]))));
        }
    }
}
