using MaceEvolve.Enums;
using System;
using System.Linq;

namespace MaceEvolve.Models
{
    public class Node
    {
        #region Properties
        public double PreviousOutput { get; protected set; } = 0;
        public virtual NodeType NodeType { get; }
        public double Bias { get; set; }
        public bool Evaluating { get; private set; }
        public CreatureInput? CreatureInput { get; set; }
        public CreatureAction? CreatureAction { get; set; }
        public int OutputCount = 0;
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

        #region Methods
        public double GenerateOutput(NeuralNetwork Network)
        {
            Evaluating = true;

            double WeightedSum = GetWeightedSum(Network);
            double Output = Globals.Sigmoid(WeightedSum + Bias);

            PreviousOutput = Output;
            Evaluating = false;

            OutputCount += 1;

            return Output;
        }
        private double GetWeightedSum(NeuralNetwork Network)
        {
            double WeightedSum = 0;
            int MyId = Network.GetNodeId(this);

            switch (NodeType)
            {
                case NodeType.Input:
                    if (CreatureInput == null) { throw new InvalidOperationException($"Node type is {NodeType} but {nameof(CreatureInput)} is null."); }

                    return Network.InputValues[CreatureInput.Value];

                case NodeType.Process:
                case NodeType.Output:
                    if (NodeType == NodeType.Output && CreatureAction == null)
                    {
                        throw new InvalidOperationException($"Node type is {NodeType} but {nameof(CreatureAction)} is null.");
                    }

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
                            SourceNodeOutput = ConnectionSourceNode.Evaluating ? ConnectionSourceNode.PreviousOutput : ConnectionSourceNode.GenerateOutput(Network);
                        }

                        WeightedSum += SourceNodeOutput * Connection.Weight;
                    }

                    return WeightedSum;

                default:
                    throw new NotImplementedException($"{nameof(NodeType)} '{NodeType}' is not implemented.");
            }
        }
        #endregion
    }
}
