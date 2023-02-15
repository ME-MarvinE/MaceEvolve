using System;

namespace MaceEvolve.Core.Models
{
    public struct Connection : IEquatable<Connection>
    {
        #region Properties
        public int SourceId { get; }
        public int TargetId { get; }
        public float Weight { get; }
        #endregion

        #region Constructors
        public Connection(int sourceId, int targetId, float weight)
        {
            SourceId = sourceId;
            TargetId = targetId;
            Weight = weight;
        }
        #endregion

        #region Methods
        public override bool Equals(object obj)
        {
            return obj is Connection connection && Equals(connection);
        }
        public bool Equals(Connection other)
        {
            return SourceId == other.SourceId &&
                TargetId == other.TargetId &&
                Weight == other.Weight;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(SourceId, TargetId, Weight);
        }
        public static bool operator == (Connection connectionLeft, Connection connectionRight)
        {
            return connectionLeft.Equals(connectionRight);
        }
        public static bool operator != (Connection connectionLeft, Connection connectionRight)
        {
            return !connectionLeft.Equals(connectionRight);
        }
        #endregion
    }
}
