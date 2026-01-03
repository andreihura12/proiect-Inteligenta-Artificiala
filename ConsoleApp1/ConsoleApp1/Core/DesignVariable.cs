using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Core
{
    public class DesignVariable
    {
        public string Name { get; }
        public double Min { get; }
        public double Max { get; }

        public DesignVariable(string name,double min,double max)
        {
            Name = name;
            Min = min;
            Max = max;
        }
        public double Clamp(double val) => Math.Max(Min, Math.Min(Max, val));
    }
}
