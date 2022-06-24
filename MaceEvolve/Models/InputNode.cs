using MaceEvolve.Enums;

namespace MaceEvolve.Models
{
    public class InputNode : Node
    {
        #region Properties
        public CreatureInput CreatureInput { get; }
        #endregion

        #region Constructors
        public InputNode(CreatureInput CreatureInput, double Bias)
            : base(NodeType.Input, Bias)
        {
            this.CreatureInput = CreatureInput;
        }
        #endregion

        #region Methods
        public override double GetWeightedSum(NeuralNetwork Network)
        {
            return Network.InputValues[CreatureInput];
        }
        #endregion
    }
}
