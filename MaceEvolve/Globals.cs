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
        public static int Map(int Num, int Min1, int Max1, int Min2, int Max2, bool WithinBounds = true)
        {
            var NewValue = (Num - Min1) / (Max1 - Min1) * (Max2 - Min2) + Min2;

            if (!WithinBounds)
            {
                return NewValue;
            }
            if (Min2 < Max2)
            {
                return Clamp(NewValue, Min2, Max2);
            }
            else
            {
                return Clamp(NewValue, Max2, Min2);
            }
        }
        public static double Map(double Num, double Min1, double Max1, double Min2, double Max2, bool WithinBounds = true)
        {
            var NewValue = (Num - Min1) / (Max1 - Min1) * (Max2 - Min2) + Min2;

            if (!WithinBounds)
            {
                return NewValue;
            }
            if (Min2 < Max2)
            {
                return Clamp(NewValue, Min2, Max2);
            }
            else
            {
                return Clamp(NewValue, Max2, Min2);
            }
        }
        public static int Clamp(int Num, int Min, int Max)
        {
            return Math.Max(Math.Min(Num, Max), Min);
        }
        public static double Clamp(double Num, double Min, double Max)
        {
            return Math.Max(Math.Min(Num, Max), Min);
        }
        #endregion
    }
}
