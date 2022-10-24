using MaceEvolve.Models;
using System;

namespace MaceEvolve.Extensions
{
    public static class RandomExtensions
    {
        public static double NextDouble(this Random Random, double MinValue, double MaxValue)
        {
            return Random.NextDouble() * (MaxValue - MinValue) + MinValue;
        }
        public static double NextDoubleVariance(this Random Random, double Value, double Variance)
        {
            double Multiplier = Random.NextDouble(1 - Variance, 1 + Variance);

            return Value * Multiplier;
        }
    }
}
