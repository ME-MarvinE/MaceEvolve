using MaceEvolve.Enums;
using MaceEvolve.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaceEvolve
{
    public static class Globals
    {
        #region Properties
        public static Random Random = new Random();
        public static ReadOnlyCollection<CreatureInput> AllCreatureInputs { get; } = Enum.GetValues(typeof(CreatureInput)).Cast<CreatureInput>().ToList().AsReadOnly();
        public static ReadOnlyCollection<CreatureAction> AllCreatureActions { get; } = Enum.GetValues(typeof(CreatureAction)).Cast<CreatureAction>().ToList().AsReadOnly();
        public static ReadOnlyCollection<NodeType> AllNodeTypes { get; } = Enum.GetValues(typeof(NodeType)).Cast<NodeType>().ToList().AsReadOnly();
        #endregion

        #region Methods
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
        public static double Sigmoid(double Num)
        {
            return 1 / (1 + Math.Exp(-Num));
        }
        public static double SigmoidDerivative(double Num)
        {
            return Num * (1 - Num);
        }
        public static int GetDistanceFrom(int X, int Y, int TargetX, int TargetY)
        {
            return (int)GetDistanceFrom((double)X, (double)Y, (double)TargetX, (double)TargetY);
        }
        public static double GetDistanceFrom(double X, double Y, double TargetX, double TargetY)
        {
            return Math.Abs(X - TargetX) + Math.Abs(Y - TargetY);
        }
        public static double Hypotenuse(int A, int B)
        {
            return Math.Sqrt(Math.Pow(A, 2) + Math.Pow(B, 2));
        }
        #endregion
    }
}
