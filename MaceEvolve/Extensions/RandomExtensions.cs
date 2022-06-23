using System;

namespace MaceEvolve.Extensions
{
    public static class RandomExtensions
    {
        public static double NextDouble(this Random Random, double MinValue, double MaxValue)
        {
            return Random.NextDouble() * (MaxValue - MinValue) + MinValue;
        }
    }
}
