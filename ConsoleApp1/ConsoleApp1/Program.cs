using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleApp1.Core;

namespace ConsoleApp1
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("=== TEST NSGA-II CORE (ZDT1) ===");

            IMultiObjectiveProblem problem = new Zdt1Problem();

            var nsga = new NSGA_II(
                problem: problem,
                populationSize: 100,
                generations: 150,
                crossoverProbability: 0.9,
                mutationProbability: 1.0 / problem.Vars.Count
            );

            var pareto = nsga.Run();

            Console.WriteLine("\nPareto front (first 10):");
            foreach (var ind in pareto.Take(10))
                Console.WriteLine($"f1={ind.F1:F4}, f2={ind.F2:F4}");

            Console.WriteLine($"\nFront1 final size = {pareto.Count}");
            Console.ReadKey();
        }
    }

    // ZDT1 test problem (standard)
    public class Zdt1Problem : IMultiObjectiveProblem
    {
        public IReadOnlyList<DesignVariable> Vars { get; } =
            Enumerable.Range(1, 10)
                .Select(i => new DesignVariable($"x{i}", 0.0, 1.0))
                .ToList();

        public (double f1, double f2) Evaluate(double[] x)
        {
            double f1 = x[0];

            double sum = 0.0;
            for (int i = 1; i < x.Length; i++)
                sum += x[i];

            double g = 1.0 + 9.0 * (sum / (x.Length - 1));
            double f2 = g * (1.0 - Math.Sqrt(f1 / g));

            return (f1, f2);
        }
    }
}
