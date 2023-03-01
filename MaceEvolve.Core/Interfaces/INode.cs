using MaceEvolve.Core.Enums;

namespace MaceEvolve.Core.Interfaces
{
    public interface INode
    {
        public NodeType NodeType { get; }
        public float Bias { get; }
    }
}
