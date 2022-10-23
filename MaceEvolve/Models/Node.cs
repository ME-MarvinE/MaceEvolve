using MaceEvolve.Enums;
using System;
using System.Linq;

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
        public Node(NodeType NodeType, double Bias, CreatureInput? CreatureInput = null, CreatureAction? CreatureAction = null)
        {
            this.NodeType = NodeType;
            this.CreatureInput = CreatureInput;
            this.CreatureAction = CreatureAction;
            this.Bias = Bias;
        }
        #endregion
    }
}
