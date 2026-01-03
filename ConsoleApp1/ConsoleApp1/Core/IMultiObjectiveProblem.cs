using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Core
{
    public interface IMultiObjectiveProblem
    {
        IReadOnlyList<DesignVariable> Vars { get; }
        (double f1, double f2) Evaluate(double[] x);
    }
}
