using MaceEvolve.Enums;
using System;
using System.Linq;

namespace MaceEvolve.Models
{
    public class Node
    {
        #region Properties
        public virtual NodeType NodeType { get; }
        public double Bias { get; set; }
        public CreatureInput? CreatureInput { get; set; }
        public CreatureAction? CreatureAction { get; set; }
        public double PreviousOutput { get; protected set; } = 0;
        public int CachedOutputCount { get; set; } = 0;
        public int UncachedOutputCount { get; set; } = 0;
        public bool Evaluating { get; private set; }
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

            double Output;

            if (Network.EvaluatedNodes.TryGetValue(this, out double CachedOutput))
            {
                Output = PreviousOutput;

                CachedOutputCount += 1;
            }
            else
            {
                double WeightedSum = GetWeightedSum(Network);

                Output = Globals.ReLU(WeightedSum + Bias);

                Network.EvaluatedNodes.Add(this, Output);

                UncachedOutputCount += 1;
            }

            PreviousOutput = Output;
            Evaluating = false;

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

                    for (int i = 0; i < Network.Connections.Count; i++)
                    {
                        Connection Connection = Network.Connections[i];

                        if (Connection.TargetId == MyId)
                        {
                            double SourceNodeOutput;
                            Node ConnectionSourceNode = Network.Nodes[Connection.SourceId];

                            if (ConnectionSourceNode.Evaluating)
                            {
                                SourceNodeOutput = ConnectionSourceNode.PreviousOutput;
                            }
                            else
                            {
                                SourceNodeOutput = ConnectionSourceNode.GenerateOutput(Network);
                            }

                            WeightedSum += SourceNodeOutput * Connection.Weight;
                        }
                    }

                    return WeightedSum;

                default:
                    throw new NotImplementedException($"{nameof(NodeType)} '{NodeType}' is not implemented.");
            }
        }
        #endregion
    }
}
