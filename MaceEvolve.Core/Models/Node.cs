using MaceEvolve.Core.Enums;

namespace MaceEvolve.Core.Models
{
    public class Node : INode
    {
        #region Properties
        public NodeType NodeType { get; }
        public float Bias { get; set; }
        public CreatureInput? CreatureInput { get; set; }
        public CreatureAction? CreatureAction { get; set; }
        #endregion

        #region Constructors
        public Node(NodeType nodeType, float bias, CreatureInput? creatureInput = null, CreatureAction? creatureAction = null)
        {
            NodeType = nodeType;
            CreatureInput = creatureInput;
            CreatureAction = creatureAction;
            Bias = bias;
        }
        #endregion
    }
}
