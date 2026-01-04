using System;
using System.Collections.Generic;
using ConsoleApp1.Core;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== INITIALIZARE ALGORITM NSGA-II ===");
            Console.WriteLine("Problema: Optimizare Design Masina (Viteza vs Consum)");
            Console.WriteLine("-----------------------------------------------------");

           
            var carProblem = new CarProblem();

            
            var algo = new NSGA_II(
                problem: carProblem,
                populationSize: 100,
                generations: 100,
                crossoverProbability: 0.9,
                mutationProbability: 0.1
            );

           
            Console.WriteLine("Se ruleaza optimizarea...");
            var solutiiOptime = algo.Run();

            
            Console.WriteLine("\nRezultate finale (front Pareto)");
            Console.WriteLine("Acestea sunt cele mai eficiente configuratii gasite:");
            Console.WriteLine("");
            Console.WriteLine("{0,-10} | {1,-10} | {2,-10} || {3,-15} | {4,-15}",
                "Putere", "Greutate", "Aero", "Viteza (km/h)", "Consum (Score)");
            Console.WriteLine(new string('-', 85));

            foreach (var s in solutiiOptime)
            {
                double cp = s.X[0];
                double kg = s.X[1];
                double aero = s.X[2];

                
                double viteza = -s.F1;
                double consum = s.F2;

                Console.WriteLine("{0,-10:F0} | {1,-10:F0} | {2,-10:F2} || {3,-15:F1} | {4,-15:F2}",
                    cp, kg, aero, viteza, consum);
            }


            Console.ReadLine();
        }
    }

    
    public class CarProblem : IMultiObjectiveProblem
    {
        public IReadOnlyList<DesignVariable> Vars { get; } = new List<DesignVariable>
        {
            new DesignVariable("Putere", 50, 400),
            new DesignVariable("Greutate", 800, 2500),
            new DesignVariable("Aerodinamica", 0.2, 0.6)
        };

        public (double f1, double f2) Evaluate(double[] x)
        {
            double putere = x[0];
            double greutate = x[1];
            double aero = x[2];
            double viteza = 22.5 * Math.Pow(putere / (aero * 0.5), 0.33);
            double consum = (putere * 0.04) + (greutate * 0.003) + (aero * 10);
            return (-viteza, consum);
        }
    }
}