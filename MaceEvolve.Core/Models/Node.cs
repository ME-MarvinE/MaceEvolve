using MaceEvolve.Core.Enums;
using MaceEvolve.Core.Interfaces;

namespace MaceEvolve.Core.Models
{
    public class Node : INode
    {
        #region Properties
        public NodeType NodeType { get; }
        public float Bias { get; }
        public CreatureInput? CreatureInput { get; }
        public CreatureAction? CreatureAction { get; }
        #endregion

        #region Constructors
        public Node(NodeType nodeType, float bias)
            : this(nodeType, bias, null, null)
        {
        }
        public Node(NodeType nodeType, float bias, CreatureInput creatureInput)
            : this(nodeType, bias, creatureInput, null)
        {
        }
        public Node(NodeType nodeType, float bias, CreatureAction creatureAction)
            : this(nodeType, bias, null, creatureAction)
        {
        }
        public Node(NodeType nodeType, float bias, CreatureInput? creatureInput, CreatureAction? creatureAction)
        {
            NodeType = nodeType;
            CreatureInput = creatureInput;
            CreatureAction = creatureAction;
            Bias = bias;
        }
        #endregion
    }
}
