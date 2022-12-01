using MaceEvolve.Core.Enums;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;

namespace MaceEvolve.Core
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
        public static int Map(int num, int min1, int max1, int min2, int max2, bool withinBounds = true)
        {
            var newValue = (num - min1) / (max1 - min1) * (max2 - min2) + min2;

            if (!withinBounds)
            {
                return newValue;
            }
            if (min2 < max2)
            {
                return Clamp(newValue, min2, max2);
            }
            else
            {
                return Clamp(newValue, max2, min2);
            }
        }
        public static double Map(double num, double min1, double max1, double min2, double max2, bool withinBounds = true)
        {
            var newValue = (num - min1) / (max1 - min1) * (max2 - min2) + min2;

            if (!withinBounds)
            {
                return newValue;
            }
            if (min2 < max2)
            {
                return Clamp(newValue, min2, max2);
            }
            else
            {
                return Clamp(newValue, max2, min2);
            }
        }
        public static int Clamp(int num, int min, int max)
        {
            return Math.Max(Math.Min(num, max), min);
        }
        public static double Clamp(double num, double min, double max)
        {
            return Math.Max(Math.Min(num, max), min);
        }
        public static double Sigmoid(double num)
        {
            return 1 / (1 + Math.Exp(-num));
        }
        public static double ReLU(double num)
        {
            return Math.Max(0, num);
        }
        public static double SigmoidDerivative(double num)
        {
            return num * (1 - num);
        }
        public static int GetDistanceFrom(int x, int y, int targetX, int targetY)
        {
            return (int)GetDistanceFrom((double)x, (double)y, (double)targetX, (double)targetY);
        }
        public static double GetDistanceFrom(double x, double y, double targetX, double targetY)
        {
            return Math.Abs(x - targetX) + Math.Abs(y - targetY);
        }
        public static double Hypotenuse(double a, double b)
        {
            return Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
        }
        public static double MiddleX(double x, double width)
        {
            return x + width / 2;
        }
        public static double MiddleY(double y, double height)
        {
            return y + height / 2;
        }
        #endregion
    }
}
