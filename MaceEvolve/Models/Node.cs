using MaceEvolve.Enums;

namespace MaceEvolve.Models
{
    public class Node
    {
        #region Properties
        public NodeType NodeType { get; }
        public double Bias { get; set; }
        public CreatureInput? CreatureInput { get; set; }
        public CreatureAction? CreatureAction { get; set; }
        #endregion

        #region Constructors
        public Node(NodeType nodeType, double bias, CreatureInput? creatureInput = null, CreatureAction? creatureAction = null)
        {
            NodeType = nodeType;
            CreatureInput = creatureInput;
            CreatureAction = creatureAction;
            Bias = bias;
        }
        #endregion
    }
}
