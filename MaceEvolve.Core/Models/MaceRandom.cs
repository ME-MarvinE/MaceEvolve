using System;

namespace MaceEvolve.Core.Models
{
    public class MaceRandom
    {
        #region Fields
        private readonly Random _random = new Random();
        [ThreadStatic]
        private static MaceRandom _maceRandom;
        #endregion

        #region Properties
        public static MaceRandom Current
        {
            get
            {
                _maceRandom ??= new MaceRandom();

                return _maceRandom;
            }
        }
        #endregion

        #region Methods
        public int Next()
        {
            return _random.Next();
        }
        public int Next(int maxValue)
        {
            return _random.Next(maxValue);
        }
        public int Next(int minValue, int maxValue)
        {
            return _random.Next(minValue, maxValue);
        }
        public float NextFloat()
        {
            return (float)_random.NextDouble();
        }
        public float NextFloat(float minValue, float maxValue)
        {
            return NextFloat() * (maxValue - minValue) + minValue;
        }
        public double NextDouble()
        {
            return _random.NextDouble();
        }
        public double NextDouble(double minValue, double maxValue)
        {
            return _random.NextDouble() * (maxValue - minValue) + minValue;
        }
        public double NextDoubleVariance(double value, double variance)
        {
            double Multiplier = NextDouble(1 - variance, 1 + variance);

            return value * Multiplier;
        }
        public float NextFloatVariance(float value, float variance)
        {
            float Multiplier = NextFloat(1 - variance, 1 + variance);

            return value * Multiplier;
        }
        public void NextBytes(byte[] buffer)
        {
            _random.NextBytes(buffer);
        }
        public void NextBytes(Span<byte> buffer)
        {
            _random.NextBytes(buffer);
        }
        #endregion
    }
}
