using MaceEvolve.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MaceEvolve.Models
{
    public class Genome
    {
        #region Properties
        protected static Random _Random { get; }
        public List<ProcessNode> Genes { get; }
        public static List<CreatureValue> CreatureInputs { get; }
        public static List<CreatureOutput> CreatureOutputs { get; }
        public static List<ProcessNode> DefaultNodeNetwork { get; }
        #endregion

        #region Constructors
        static Genome()
        {
            _Random = new Random();
            CreatureInputs = Enum.GetValues(typeof(CreatureValue)).Cast<CreatureValue>().ToList();
            CreatureOutputs = Enum.GetValues(typeof(CreatureOutput)).Cast<CreatureOutput>().ToList();

            DefaultNodeNetwork = GenerateNodeNetwork(1, 4, _Random.Next(4), NodeType.Output);
        }
        public Genome()
            : this(new List<ProcessNode>(DefaultNodeNetwork))
        {
        }
        public Genome(List<ProcessNode> Genes)
        {
            this.Genes = Genes;
        }
        #endregion

        #region Methods
        public static double GetRandomWeight()
        {
            int Bound = 5;
            bool IsNegative = _Random.Next(0, 2) == 1;
            return _Random.NextDouble() * (IsNegative ? Bound : -Bound);
        }
        public static List<ProcessNode> GenerateNodeNetwork(int MinBreadth, int MaxBreadth, int HiddenLayerCount, NodeType LayerType)
        {
            List<ProcessNode> ProcessNodes;
            bool HasHiddenLayers = HiddenLayerCount > 0;
            int RandomBreadth = _Random.Next(MinBreadth, MaxBreadth);

            switch (LayerType)
            {
                case NodeType.Input:
                    ProcessNodes = GenerateNodes(RandomBreadth, NodeType.Input);
                    break;

                case NodeType.Output:
                    ProcessNodes = GenerateNodes(RandomBreadth, NodeType.Output);

                    if (HasHiddenLayers)
                    {
                        foreach (ProcessNode ProcessNode in ProcessNodes)
                        {
                            ProcessNode.Inputs = GenerateNodeNetwork(MinBreadth, MaxBreadth, HiddenLayerCount, NodeType.Process);
                        }
                    }
                    else
                    {
                        foreach (ProcessNode ProcessNode in ProcessNodes)
                        {
                            ProcessNode.Inputs = GenerateNodes(RandomBreadth, NodeType.Input);
                        }
                    }
                    break;

                case NodeType.Process:
                    if (HasHiddenLayers)
                    {
                        ProcessNodes = GenerateNodes(RandomBreadth, NodeType.Process);
                        foreach (ProcessNode ProcessNode in ProcessNodes)
                        {
                            ProcessNode.Inputs = GenerateNodeNetwork(MinBreadth, MaxBreadth, HiddenLayerCount - 1, NodeType.Process);
                        }
                    }
                    else
                    {
                        ProcessNodes = GenerateNodes(RandomBreadth, NodeType.Input);
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }

            return ProcessNodes;
        }
        public static List<ProcessNode> GenerateNodes(int TargetAmount, NodeType NodeType)
        {
            List<ProcessNode> ProcessNodes = new List<ProcessNode>();
            List<double> RandomNodeWeights = GenerateRandomWeights(TargetAmount);

            switch (NodeType)
            {
                case NodeType.Input:
                    List<CreatureValue> RandomCreatureInputs = GenerateRandomInputs(TargetAmount, CreatureInputs, true);

                    for (int i = 0; i < RandomCreatureInputs.Count; i++)
                    {
                        ProcessNodes.Add(new ProcessNode()
                        {
                            ConnectionWeight = RandomNodeWeights[i],
                            InputNodeCreatureInput = RandomCreatureInputs[i],
                            NodeType = NodeType
                        });
                    }
                    break;

                case NodeType.Output:
                    List<CreatureOutput> RandomCreatureOutputs = GenerateRandomOutputs(TargetAmount, CreatureOutputs, true);

                    for (int i = 0; i < RandomCreatureOutputs.Count; i++)
                    {
                        ProcessNodes.Add(new ProcessNode()
                        {
                            ConnectionWeight = RandomNodeWeights[i],
                            OutputNodeCreatureOutput = RandomCreatureOutputs[i],
                            NodeType = NodeType
                        });
                    }
                    break;

                case NodeType.Process:
                    for (int i = 0; i < TargetAmount; i++)
                    {
                        ProcessNodes.Add(new ProcessNode()
                        {
                            ConnectionWeight = RandomNodeWeights[i],
                            NodeType = NodeType
                        });
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            return ProcessNodes;
        }
        public static List<double> GenerateRandomWeights(int TargetAmount)
        {
            List<double> Weights = new List<double>();

            for (int i = 0; i < TargetAmount; i++)
            {
                Weights.Add(GetRandomWeight());
            }

            return Weights;
        }
        public static List<CreatureOutput> GenerateRandomOutputs(int TargetAmount, IEnumerable<CreatureOutput> PossibleOutputs, bool Distinct)
        {
            List<CreatureOutput> Outputs = new List<CreatureOutput>();
            List<CreatureOutput> PossibleOutputsList = PossibleOutputs.ToList();

            if (Distinct)
            {
                TargetAmount = TargetAmount < PossibleOutputsList.Count ? TargetAmount : PossibleOutputsList.Count;
            }

            for (int i = 0; i < TargetAmount; i++)
            {
                int RandomIndex = _Random.Next(PossibleOutputsList.Count);
                Outputs.Add(PossibleOutputsList[RandomIndex]);
                if (Distinct)
                {
                    PossibleOutputsList.Remove(PossibleOutputsList[RandomIndex]);
                }
            }

            return Outputs;
        }
        public static List<CreatureValue> GenerateRandomInputs(int TargetAmount, IEnumerable<CreatureValue> PossibleInputs, bool Distinct)
        {
            List<CreatureValue> Inputs = new List<CreatureValue>();
            List<CreatureValue> PossibleInputsList = PossibleInputs.ToList();

            if (Distinct)
            {
                TargetAmount = TargetAmount < PossibleInputsList.Count ? TargetAmount : PossibleInputsList.Count;
            }

            for (int i = 0; i < TargetAmount; i++)
            {
                int RandomIndex = _Random.Next(PossibleInputsList.Count);
                Inputs.Add(PossibleInputsList[RandomIndex]);
                if (Distinct)
                {
                    PossibleInputsList.Remove(PossibleInputsList[RandomIndex]);
                }
            }

            return Inputs;
        }
        #endregion
    }
}
