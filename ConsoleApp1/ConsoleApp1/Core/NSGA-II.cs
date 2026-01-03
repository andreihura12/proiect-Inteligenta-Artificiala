using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Core
{
    public class NSGA_II
    {
        private readonly IMultiObjectiveProblem _problem;
        private readonly Random _rng;

        public int PopulationSize { get; }
        public int Generations { get; }
        public double CrossoverProbability { get; }
        public double MutationProbability { get; }
        public NSGA_II(IMultiObjectiveProblem problem, int populationSize, int generations, double crossoverProbability=0.9, double mutationProbability=0.1,int? seed = null)
        {
            _problem = problem;
            _rng = seed.HasValue ? new Random(seed.Value) : new Random();
            PopulationSize = populationSize;
            Generations = generations;
            CrossoverProbability = crossoverProbability;
            MutationProbability = mutationProbability;
            
        }
        public List<Individual> Run()
        {
            var pop= InitPopulation();
            Evaluate(pop);
            AssignRankAndCrowding(pop);

            for (int gen = 0; gen < Generations; gen++)
            {
                var offspring = MakeOffspring(pop);
                Evaluate(offspring);
                var combined = pop.Concat(offspring).ToList();
                AssignRankAndCrowding(combined);

                pop = SelectNextPopulation(combined,PopulationSize);
                if((gen+1)% Math.Max(1,Generations/10)==0)
                {
                    var bestFront = pop.Where(i => i.Rank == 1).ToList();
                    Console.WriteLine($"Gen {gen + 1}/{Generations} | Front1 size: {bestFront.Count}");
                }
            }
            return pop.Where(i => i.Rank == 1).OrderBy(i=>i.F1).ToList();
        }
        private List<Individual> InitPopulation()
        {
            int n = _problem.Vars.Count;
            var pop = new List<Individual>(PopulationSize);
            for (int i=0; i< PopulationSize; i++)
            {
                var ind = new Individual(n);
                for (int j = 0; j < n; j++)
                {
                    var v= _problem.Vars[j];
                    ind.X[j]=v.Min+ _rng.NextDouble() * (v.Max - v.Min);
                }
                pop.Add(ind);
            }
            return pop;
        }
        private void Evaluate(List<Individual> pop)
        {
            foreach (var ind in pop)
            {
                (ind.F1, ind.F2) = _problem.Evaluate(ind.X);
                
            }
        }
        private void AssignRankAndCrowding(List<Individual> pop)
        {
            foreach(var p in pop)
            {
                p.Dominates.Clear();
                p.DominatedCount = 0;
                p.Rank = 0;
                p.Crowding = 0;
            }
            var fronts = FastNonDominatedSort(pop);
            for(int i=0;i<fronts.Count;i++)
            {
                int rank = i + 1;
                foreach (var ind in fronts[i]) ind.Rank = rank;
                ComputeCrowdingDistance(fronts[i]);
            }
        }
        private List<List<Individual>> FastNonDominatedSort(List<Individual> pop)
        {
            var fronts = new List<List<Individual>>();
            var firstFront = new List<Individual>();
            for(int i=0;i<pop.Count;i++)
            {
                var p = pop[i];
                for(int j=0; j<pop.Count;j++)
                {
                    if(i==j) continue;
                    var q = pop[j];
                    if (Dominates(p, q))
                        p.Dominates.Add(q);
                    else if (Dominates(q, p))
                        p.DominatedCount++;
                }
                if (p.DominatedCount == 0)
                    firstFront.Add(p);
            }
            fronts.Add(firstFront);
            int k = 0;
            while (k < fronts.Count && fronts[k].Count > 0)
            {
                var next = new List<Individual>();
                foreach (var p in fronts[k])
                {
                    foreach (var q in p.Dominates)
                    {
                        q.DominatedCount--;
                        if (q.DominatedCount == 0)
                            next.Add(q);
                    }
                }
                if (next.Count > 0) fronts.Add(next);
                k++;
            }
            return fronts;
        }
        private static bool Dominates(Individual p,Individual q)
        {
            bool notworse=(p.F1 <= q.F1 && p.F2 <= q.F2);
            bool strictlybetter=(p.F1 < q.F1 || p.F2 < q.F2);
            return notworse && strictlybetter;
        }
        private static void ComputeCrowdingDistance(List<Individual> front)
        {
            if (front.Count == 0) return;
            foreach (var ind in front) ind.Crowding = 0;
            front.Sort((a,b)=> a.F1.CompareTo(b.F1));
            front[0].Crowding = double.PositiveInfinity;
            front[front.Count - 1].Crowding = double.PositiveInfinity;
            double minF1 = front[0].F1, maxF1 = front[front.Count - 1].F1;

            for(int i=1;i<front.Count-1; i++)
            {
               double denom= maxF1 - minF1;
                if(denom> 1e-12)
                    front[i].Crowding += (front[i + 1].F1 - front[i - 1].F1) / denom;
            }
            front.Sort((a, b) => a.F2.CompareTo(b.F2));
            front[0].Crowding = double.PositiveInfinity;
            front[front.Count - 1].Crowding = double.PositiveInfinity;
            double minF2 = front[0].F2, maxF2 = front[front.Count - 1].F2;

            for(int i=1;i<front.Count-1;i++)
            {
                double denom = (maxF2 - minF2);
                if ( denom > 1e-12)
                    front[i].Crowding += (front[i + 1].F2 - front[i - 1].F2) / denom;

            }
        }
        private List<Individual> MakeOffspring(List<Individual>pop)
        {
            var kids = new List<Individual>(PopulationSize);
            while (kids.Count<PopulationSize)
            {
                var p1 = TournamentSelect(pop);
                var p2 = TournamentSelect(pop);

                var c1= p1.Clone();
                var c2= p2.Clone();

                if(_rng.NextDouble() < CrossoverProbability)
                    SimulatedBinaryCrossover(c1.X, c2.X);
                PolynomialMutation(c1.X);
                PolynomialMutation(c2.X);

                kids.Add(c1);
                if (kids.Count < PopulationSize)
                    kids.Add(c2);
            }
            return kids;
        }
        private Individual TournamentSelect(List<Individual>pop)
        {
            var a = pop[_rng.Next(pop.Count)];
            var b = pop[_rng.Next(pop.Count)];

            if (a.Rank < b.Rank) return a;
            if (b.Rank < a.Rank) return b;
            return (a.Crowding > b.Crowding) ? a : b;

        }
        private void SimulatedBinaryCrossover(double[] x1,double[] x2, double eta=15.0)
        {
            for(int i=0;i<x1.Length;i++)
            {
                if (_rng.NextDouble() > 0.5) continue;
                double u = _rng.NextDouble();
                double beta= ( u<=0.5)
                    
                    ? Math.Pow(2 * u, 1.0 / (eta + 1)) 
                    : Math.Pow(1 / (2 * (1 - u)), 1.0 / (eta + 1));
                double c1= 0.5 * ((1 + beta) * x1[i] + (1 - beta) * x2[i]);
                double c2= 0.5 * ((1 - beta) * x1[i] + (1 + beta) * x2[i]);

                var v= _problem.Vars[i];
                x1[i] = v.Clamp(c1);
                x2[i] = v.Clamp(c2);
            }
        }
        private void PolynomialMutation(double[] x, double eta=20.0)
        {
            for (int i=0;i<x.Length;i++)
            {
                if (_rng.NextDouble() > MutationProbability) continue;
                var v= _problem.Vars[i];
                double y=x[i];
                double yl= v.Min;
                double yu= v.Max;

                if (yu - yl < 1e-12) continue;

                double delta1 = (y - yl) / (yu - yl);
                double delta2 = (yu - y) / (yu - yl);
                double r = _rng.NextDouble();
                double mutPow = 1.0 / (eta + 1.0);

                double deltaq;
                if(r<=0.5)
                {
                    double xy = 1.0 - delta1;
                    double val= 2.0 * r + (1.0 - 2.0 * r) * Math.Pow(xy, eta + 1.0);
                    deltaq = Math.Pow(val, mutPow) - 1.0;

                }
                else
                {
                    double xy = 1.0 - delta2;
                    double val = 2.0 * (1.0 - r) + 2.0 * (r - 0.5) * Math.Pow(xy, eta + 1.0);
                    deltaq = 1.0 - Math.Pow(val, mutPow);
                }
                y = y + deltaq * (yu - yl);
                x[i] = v.Clamp(y);

            }
        }
        private List<Individual> SelectNextPopulation(List<Individual> combined,int targetSize)
        {
            var groups = combined.GroupBy(i => i.Rank).OrderBy(g => g.Key);
            var next = new List<Individual>(targetSize);
            foreach (var g in groups)
            {
                var front = g.ToList();
                if (next.Count + front.Count <= targetSize)
                {
                    next.AddRange(front);
                }
                else
                {
                    front.Sort((a, b) => b.Crowding.CompareTo(a.Crowding));
                    int remaining = targetSize - next.Count;
                    next.AddRange(front.Take(remaining));
                    break;
                }
            }
            return next;
        }
    }
}
