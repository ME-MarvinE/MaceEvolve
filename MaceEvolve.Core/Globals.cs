using MaceEvolve.Core.Enums;
using MaceEvolve.Core.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;

namespace MaceEvolve.Core
{
    public static class Globals
    {
        #region Fields
        private const double Rad2Deg = 180 / Math.PI;
        private const double Deg2Rad = Math.PI / 180;
        private const float Rad2DegF = 180 / MathF.PI;
        private const float Deg2RadF = MathF.PI / 180;
        #endregion

        #region Properties
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
        public static int GetDistanceFrom(int x1, int y1, int x2, int y2)
        {
            return (int)GetDistanceFrom((float)x1, y1, x2, y2);
        }
        public static double GetDistanceFrom(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }
        public static float GetDistanceFrom(float x1, float y1, float x2, float y2)
        {
            return MathF.Sqrt(MathF.Pow(x1 - x2, 2) + MathF.Pow(y1 - y2, 2));
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
        public static bool ShouldCreatureBeDead(ICreature creature)
        {
            if (creature.Energy <= 0 || creature.HealthPoints <= 0 || creature.Mass <= 0 || creature.Age > creature.MaxAge)
            {
                return true;
            }

            return false;
        }
        public static double AngleToRadians(double angle)
        {
            return Deg2Rad * angle;
        }
        public static float AngleToRadians(float angle)
        {
            return Deg2RadF * angle;
        }
        public static double RadiansToAngle(double radians)
        {
            return Rad2Deg * radians;
        }
        public static float RadiansToAngle(float radians)
        {
            return Rad2DegF * radians;
        }
        public static double GetAngleBetween(Point start, Point end)
        {
            return Math.Atan2(start.Y - end.Y, end.X - start.X) * Rad2Deg;
        }
        public static double GetAngleBetween(PointF start, PointF end)
        {
            return Math.Atan2(start.Y - end.Y, end.X - start.X) * Rad2Deg;
        }
        public static float GetAngleBetweenF(Point start, Point end)
        {
            return MathF.Atan2(start.Y - end.Y, end.X - start.X) * Rad2DegF;
        }
        public static float GetAngleBetweenF(PointF start, PointF end)
        {
            return MathF.Atan2(start.Y - end.Y, end.X - start.X) * Rad2DegF;
        }
        public static float GetAngleBetweenF(float x1, float y1, float x2, float y2)
        {
            return GetAngleBetweenF(new PointF(x1, y1), new PointF(x2, y2));
        }
        public static double AngleDifference(double angle1, double angle2)
        {
            return 180 - Math.Abs(Math.Abs(angle1 - angle2) - 180);
        }
        public static float AngleDifference(float angle1, float angle2)
        {
            return 180 - MathF.Abs(MathF.Abs(angle1 - angle2) - 180);
        }
        #endregion
    }
}
