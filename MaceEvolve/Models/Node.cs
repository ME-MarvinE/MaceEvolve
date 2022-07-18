using MaceEvolve.Enums;
using System.Linq;

namespace MaceEvolve.Models
{
    public abstract class Node
    {
        #region Properties
        public double PreviousOutput { get; protected set; } = 0;
        public virtual NodeType NodeType { get; }
        public double Bias { get; set; }
        public bool Evaluating { get; private set; }
        #endregion

        #region Constructors
        protected Node(NodeType NodeType, double Bias)
        {
            this.NodeType = NodeType;
            this.Bias = Bias;
        }
        #endregion

        #region Methods
        public double EvaluateValue(NeuralNetwork Network)
        {
            Evaluating = true;

            double WeightedSum = GetWeightedSum(Network);
            double Output = Globals.Sigmoid(WeightedSum + Bias);

            PreviousOutput = Output;
            Evaluating = false;

            return Output;
        }
        public abstract double GetWeightedSum(NeuralNetwork Network);
        #endregion
    }
}
