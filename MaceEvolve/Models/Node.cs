using MaceEvolve.Enums;
using System.Linq;

namespace MaceEvolve.Models
{
    public class Node
    {
        #region Properties
        public double PreviousOutput { get; protected set; } = 0;
        public NodeType NodeType { get; }
        public CreatureValue CreatureValue { get; }
        public CreatureOutput CreatureOutput { get; }
        public double Bias { get; }
        #endregion

        #region Constructors
        public Node(double Bias)
        {
            this.Bias = Bias;
            NodeType = NodeType.Process;
        }
        public Node(CreatureValue CreatureValue, double Bias)
        {
            this.Bias = Bias;
            NodeType = NodeType.Input;
            this.CreatureValue = CreatureValue;
        }
        public Node(CreatureOutput CreatureOutput, double Bias)
        {
            this.Bias = Bias;
            NodeType = NodeType.Output;
            this.CreatureOutput = CreatureOutput;
        }
        #endregion

        #region Methods
        public double EvaluateValue(NeuralNetwork Network)
        {
            int MyId = Network.Nodes.First(x => x.Value == this).Key;
            double WeightedSum = 0;

            if (NodeType == NodeType.Input)
            {
                WeightedSum += Network.InputValues[CreatureValue];
            }
            else
            {
                foreach (var Connection in Network.Connections.Where(x => x.TargetId == MyId))
                {
                    double SourceNodeOutput;

                    if (Connection.SourceId == MyId)
                    {
                        SourceNodeOutput = PreviousOutput;
                    }
                    else
                    {
                        SourceNodeOutput = Network.Nodes[Connection.SourceId].EvaluateValue(Network);
                    }

                    WeightedSum += SourceNodeOutput * Connection.Weight;
                }
            }

            double Output = Globals.Sigmoid(WeightedSum + Bias);
            PreviousOutput = Output;
            return Output;
        }
        #endregion
    }
}
