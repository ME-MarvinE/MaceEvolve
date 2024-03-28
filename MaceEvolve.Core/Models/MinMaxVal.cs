using System;

namespace MaceEvolve.Core.Models
{
    public readonly struct MinMaxVal
    {
        public static MinMaxVal<T> Create<T>(T min, T max)
        {
            return new MinMaxVal<T>(min, max);
        }
    }
    public readonly struct MinMaxVal<T> : IEquatable<MinMaxVal<T>>
    {
        #region Properties
        public T Min { get; }
        public T Max { get; }
        #endregion

        #region Constructors
        public MinMaxVal(T min, T max)
        {
            Min = min;
            Max = max;
        }
        #endregion

        #region Methods
        public bool Equals(MinMaxVal<T> other)
        {
            return Min.Equals(other.Min) && Max.Equals(other.Max);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Min, Max);
        }
        #endregion
    }
}
