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
            int newValue = (num - min1) / (max1 - min1) * (max2 - min2) + min2;

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
        public static float Map(float num, float min1, float max1, float min2, float max2, bool withinBounds = true)
        {
            float newValue = (num - min1) / (max1 - min1) * (max2 - min2) + min2;

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
        public static float Clamp(float num, float min, float max)
        {
            return MathF.Max(MathF.Min(num, max), min);
        }
        public static double Sigmoid(double num)
        {
            return 1 / (1 + Math.Exp(-num));
        }
        public static float Sigmoid(float num)
        {
            return 1 / (1 + MathF.Exp(-num));
        }
        public static double ReLU(double num)
        {
            return Math.Max(0, num);
        }
        public static float ReLU(float num)
        {
            return MathF.Max(0, num);
        }
        public static double SigmoidDerivative(double num)
        {
            return num * (1 - num);
        }
        public static float SigmoidDerivative(float num)
        {
            return num * (1 - num);
        }
        public static int GetDistanceFrom(int x, int y, int targetX, int targetY)
        {
            return (int)GetDistanceFrom((double)x, y, targetX, targetY);
        }
        public static double GetDistanceFrom(double x, double y, double targetX, double targetY)
        {
            return Math.Abs(x - targetX) + Math.Abs(y - targetY);
        }
        public static float GetDistanceFrom(float x, float y, float targetX, float targetY)
        {
            return MathF.Abs(x - targetX) + MathF.Abs(y - targetY);
        }
        public static double Hypotenuse(double a, double b)
        {
            return Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
        }
        public static float Hypotenuse(float a, float b)
        {
            return MathF.Sqrt(MathF.Pow(a, 2) + MathF.Pow(b, 2));
        }
        public static double MiddleX(double x, double width)
        {
            return x + width / 2;
        }
        public static float MiddleX(float x, float width)
        {
            return x + width / 2;
        }
        public static double MiddleY(double y, double height)
        {
            return y + height / 2;
        }
        public static float MiddleY(float y, float height)
        {
            return y + height / 2;
        }
        #endregion
    }
}
