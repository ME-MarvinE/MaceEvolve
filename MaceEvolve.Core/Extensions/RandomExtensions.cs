using System;

namespace MaceEvolve.Core.Extensions
{
    public static class RandomExtensions
    {
        public static double NextDouble(this Random random, double minValue, double maxValue)
        {
            return random.NextDouble() * (maxValue - minValue) + minValue;
        }
        public static float NextFloat(this Random random)
        {
            return (float)random.NextDouble();
        }
        public static float NextFloat(this Random random, float minValue, float maxValue)
        {
            return random.NextFloat() * (maxValue - minValue) + minValue;
        }
        public static double NextDoubleVariance(this Random random, double value, double variance)
        {
            double Multiplier = random.NextDouble(1 - variance, 1 + variance);

            return value * Multiplier;
        }
        public static float NextFloatVariance(this Random random, float value, float variance)
        {
            float Multiplier = random.NextFloat(1 - variance, 1 + variance);

            return value * Multiplier;
        }
    }
}
