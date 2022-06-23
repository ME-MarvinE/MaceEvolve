using MaceEvolve.Enums;
using System.Linq;

namespace MaceEvolve.Models
{
    public class Node
    {
        #region Properties
        public double PreviousOutput { get; protected set; } = 0;
        public NodeType NodeType { get; }
        public CreatureInput CreatureValue { get; }
        public CreatureAction CreatureAction { get; }
        public double Bias { get; }
        private bool Evaluating { get; set; }
        #endregion

        #region Constructors
        public Node(double Bias)
        {
            this.Bias = Bias;
            NodeType = NodeType.Process;
        }
        public Node(CreatureInput CreatureValue, double Bias)
        {
            this.Bias = Bias;
            NodeType = NodeType.Input;
            this.CreatureValue = CreatureValue;
        }
        public Node(CreatureAction CreatureAction, double Bias)
        {
            this.Bias = Bias;
            NodeType = NodeType.Output;
            this.CreatureAction = CreatureAction;
        }
        #endregion

        #region Methods
        public double EvaluateValue(NeuralNetwork Network)
        {
            Evaluating = true;
            int MyId = Network.Nodes.IndexOf(this);
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
            }

            double Output = Globals.Sigmoid(WeightedSum + Bias);
            PreviousOutput = Output;
            Evaluating = false;
            return Output;
        }
        #endregion
    }
}
