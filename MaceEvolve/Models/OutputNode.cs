using MaceEvolve.Enums;
using System.Linq;

namespace MaceEvolve.Models
{
    public class OutputNode : Node
    {
        #region Properties
        public CreatureAction CreatureAction { get; }
        #endregion

        #region Constructors
        public OutputNode(CreatureAction CreatureAction, double Bias)
            : base(NodeType.Output, Bias)
        {
            this.CreatureAction = CreatureAction;
        }
        #endregion

        #region Methods
        public override double GetWeightedSum(NeuralNetwork Network)
        {
            double WeightedSum = 0;
            int MyId = Network.Nodes.IndexOf(this);

            foreach (var Connection in Network.Connections.Where(x => x.TargetId == MyId))
            {
                double SourceNodeOutput;
                Node ConnectionSourceNode = Network.Nodes[Connection.SourceId];

                if (Connection.SourceId == MyId)
                {
                    SourceNodeOutput = PreviousOutput;
                }
                else
                {
                    SourceNodeOutput = ConnectionSourceNode.Evaluating ? ConnectionSourceNode.PreviousOutput : ConnectionSourceNode.EvaluateValue(Network);
                }

                WeightedSum += SourceNodeOutput * Connection.Weight;
            }

            return WeightedSum;
        }
        #endregion
    }
}
