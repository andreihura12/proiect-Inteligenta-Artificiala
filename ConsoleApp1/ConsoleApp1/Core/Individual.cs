using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Core
{
    public class Individual
    {
        public double[] X;
        public double F1, F2;
        public int Rank;
        public double Crowding;
        public List<Individual> Dominates = new List<Individual>();
        public int DominatedCount = 0;

        public Individual(int nVars)
        {
           X= new double[nVars];
        }
        public Individual Clone()
        {
            return new Individual(X.Length)
            {
                X = (double[])X.Clone(),
                F1 = F1,
                F2 = F2,
                Rank = Rank,
                Crowding = Crowding
            };
        }
    }
}
