using System;

namespace MaceEvolve.Extensions
{
    public static class RandomExtensions
    {
        public static double NextDouble(this Random random, double minValue, double maxValue)
        {
            return random.NextDouble() * (maxValue - minValue) + minValue;
        }
        public static double NextDoubleVariance(this Random random, double value, double variance)
        {
            double Multiplier = random.NextDouble(1 - variance, 1 + variance);

            return value * Multiplier;
        }
    }
}
