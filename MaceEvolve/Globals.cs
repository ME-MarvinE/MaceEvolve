using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaceEvolve
{
    public static class Globals
    {
        #region Methods
        public static double ToPositive(double Num1)
        {
            return Math.Sqrt(Math.Pow(Num1, 2));
        }
        public static int ToPositive(int Num1)
        {
            return (int)ToPositive((double)Num1);
        }
        #endregion
    }
}
