﻿using MaceEvolve.Enums;
using MaceEvolve.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MaceEvolve.Models
{
    public class NeuralNetwork
    {
        #region Properties
        public List<CreatureInput> Inputs { get; } = new List<CreatureInput>();
        public Dictionary<CreatureInput, double> InputValues { get; } = new Dictionary<CreatureInput, double>();
        public List<CreatureAction> Actions { get; } = new List<CreatureAction>();
        public List<InputNode> InputNodes { get; } = new List<InputNode>();
        public List<ProcessNode> ProcessNodes { get; } = new List<ProcessNode>();
        public List<OutputNode> OutputNodes { get; } = new List<OutputNode>();
        [JsonIgnore]
        public List<Node> Nodes { get; } = new List<Node>();
        public List<Connection> Connections { get; set; } = new List<Connection>();
        #endregion

        #region Constructors
        public NeuralNetwork(IEnumerable<CreatureInput> Inputs, int MaxProcessNodes, IEnumerable<CreatureAction> Actions, int MinConnections, int MaxConnections, double ConnectionWeightBound)
            : this(GenerateInputNodes(Inputs), GenerateProcessNodes(MaxProcessNodes), GenerateOutputNodes(Actions), MinConnections, MaxConnections, ConnectionWeightBound)
        {
        }
        public NeuralNetwork(IEnumerable<CreatureInput> Inputs, int MaxProcessNodes, IEnumerable<CreatureAction> Actions, IEnumerable<Connection> Connections)
            : this(GenerateInputNodes(Inputs), GenerateProcessNodes(MaxProcessNodes), GenerateOutputNodes(Actions), Connections)
        {
        }
        public NeuralNetwork(IEnumerable<Node> Nodes, int MinConnections, int MaxConnections, double ConnectionWeightBound)
            : this(Nodes.Where(x => x.NodeType == NodeType.Input).Cast<InputNode>(), Nodes.Where(x => x.NodeType == NodeType.Process).Cast<ProcessNode>(), Nodes.Where(x => x.NodeType == NodeType.Output).Cast<OutputNode>(), MinConnections, MaxConnections, ConnectionWeightBound)
        {
        }
        public NeuralNetwork(IEnumerable<InputNode> InputNodes, IEnumerable<ProcessNode> ProcessNodes, IEnumerable<OutputNode> OutputNodes, int MinConnections, int MaxConnections, double ConnectionWeightBound)
            : this(InputNodes, ProcessNodes, OutputNodes, InputNodes.Select(x => x.CreatureInput), OutputNodes.Select(x => x.CreatureAction), MinConnections, MaxConnections, ConnectionWeightBound)
        {
        }
        public NeuralNetwork(IEnumerable<Node> Nodes, IEnumerable<Connection> Connections)
            : this(Nodes.Where(x => x.NodeType == NodeType.Input).Cast<InputNode>(), Nodes.Where(x => x.NodeType == NodeType.Process).Cast<ProcessNode>(), Nodes.Where(x => x.NodeType == NodeType.Output).Cast<OutputNode>(), Connections)
        {
        }
        public NeuralNetwork(IEnumerable<InputNode> InputNodes, IEnumerable<ProcessNode> ProcessNodes, IEnumerable<OutputNode> OutputNodes, IEnumerable<Connection> Connections)
            : this(InputNodes, ProcessNodes, OutputNodes, InputNodes.Select(x => x.CreatureInput), OutputNodes.Select(x => x.CreatureAction), Connections)
        {
        }
        public NeuralNetwork(IEnumerable<Node> Nodes, IEnumerable<CreatureInput> Inputs, IEnumerable<CreatureAction> Actions, int MinConnections, int MaxConnections, double ConnectionWeightBound)
            : this(Nodes.Where(x => x.NodeType == NodeType.Input).Cast<InputNode>(), Nodes.Where(x => x.NodeType == NodeType.Process).Cast<ProcessNode>(), Nodes.Where(x => x.NodeType == NodeType.Output).Cast<OutputNode>(), Inputs, Actions, MinConnections, MaxConnections, ConnectionWeightBound)
        {
        }
        public NeuralNetwork(IEnumerable<InputNode> InputNodes, IEnumerable<ProcessNode> ProcessNodes, IEnumerable<OutputNode> OutputNodes, IEnumerable<CreatureInput> Inputs, IEnumerable<CreatureAction> Actions, int MinConnections, int MaxConnections, double ConnectionWeightBound)
            : this(InputNodes, ProcessNodes, OutputNodes, Inputs, Actions, GenerateRandomConnections(MinConnections, MaxConnections, new List<Node>().Concat(InputNodes).Concat(ProcessNodes).Concat(OutputNodes), ConnectionWeightBound))
        {
        }
        public NeuralNetwork(IEnumerable<Node> Nodes, IEnumerable<CreatureInput> Inputs, IEnumerable<CreatureAction> Actions, IEnumerable<Connection> Connections)
            : this(Nodes.Where(x => x.NodeType == NodeType.Input).Cast<InputNode>(), Nodes.Where(x => x.NodeType == NodeType.Process).Cast<ProcessNode>(), Nodes.Where(x => x.NodeType == NodeType.Output).Cast<OutputNode>(), Inputs, Actions, Connections)
        {
        }
        [JsonConstructor]
        public NeuralNetwork(IEnumerable<InputNode> InputNodes, IEnumerable<ProcessNode> ProcessNodes, IEnumerable<OutputNode> OutputNodes, IEnumerable<CreatureInput> Inputs, IEnumerable<CreatureAction> Actions, IEnumerable<Connection> Connections)
        {
            this.Inputs = new List<CreatureInput>(Inputs);
            this.Actions = new List<CreatureAction>(Actions);
            this.InputNodes = new List<InputNode>(InputNodes);
            this.ProcessNodes = new List<ProcessNode>(ProcessNodes);
            this.OutputNodes = new List<OutputNode>(OutputNodes);
            this.Connections = new List<Connection>(Connections);

            Nodes.AddRange(this.InputNodes);
            Nodes.AddRange(this.ProcessNodes);
            Nodes.AddRange(this.OutputNodes);

            foreach (var Input in Inputs)
            {
                InputValues.Add(Input, 0);
            }
        }
        #endregion

        #region Methods
        public static List<Connection> GenerateRandomConnections(int MinConnections, int MaxConnections, IEnumerable<Node> Nodes, double WeightBound)
        {
            List<Connection> GeneratedConnections = new List<Connection>();
            int TargetConnectionAmount = Globals.Random.Next(MinConnections, MaxConnections + 1);

            List<Node> NodesList = new List<Node>(Nodes);

            Dictionary<int, Node> PossibleSourceNodes = GetPossibleSourceNodes(NodesList).ToDictionary(x => NodesList.IndexOf(x), x => x);
            Dictionary<int, Node> PossibleTargetNodes = GetPossibleTargetNodes(NodesList).ToDictionary(x => NodesList.IndexOf(x), x => x);

            if (PossibleSourceNodes.Count == 0) { throw new InvalidOperationException("No possible source nodes."); }
            if (PossibleTargetNodes.Count == 0) { throw new InvalidOperationException("No possible target nodes."); }

            while (GeneratedConnections.Count < TargetConnectionAmount)
            {
                int RandomConnectionSource = PossibleSourceNodes.Keys.ToList()[Globals.Random.Next(0, PossibleSourceNodes.Count)];
                int RandomConnectionTarget = PossibleTargetNodes.Keys.ToList()[Globals.Random.Next(0, PossibleTargetNodes.Count)];

                Connection NewConnection = new Connection() { SourceId = RandomConnectionSource, TargetId = RandomConnectionTarget, Weight = Globals.Random.NextDouble(-WeightBound, WeightBound) };
                GeneratedConnections.Add(NewConnection);
            }

            return GeneratedConnections;
        }
        public static List<InputNode> GenerateInputNodes(IEnumerable<CreatureInput> PossibleInputs)
        {
            return PossibleInputs.Select(x => new InputNode(x, Globals.Random.NextDouble(-1, 1))).ToList();
        }
        public static List<OutputNode> GenerateOutputNodes(IEnumerable<CreatureAction> PossibleOutputs)
        {
            return PossibleOutputs.Select(x => new OutputNode(x, Globals.Random.NextDouble(-1, 1))).ToList();
        }
        public static List<ProcessNode> GenerateProcessNodes(int MaxProcessNodes)
        {
            List<ProcessNode> ProcessNodes = new List<ProcessNode>();

            for (int i = 0; i < MaxProcessNodes; i++)
            {
                ProcessNodes.Add(new ProcessNode(Globals.Random.NextDouble(-1, 1)));
            }

            return ProcessNodes;
        }
        public NeuralNetwork CloneNetwork()
        {
            return JsonConvert.DeserializeObject<NeuralNetwork>(JsonConvert.SerializeObject(this));
        }
        public static void MutateNodeBiases(double MutationChancePerNode, IEnumerable<Node> Nodes)
        {
            foreach (var Node in Nodes)
            {
                if (Globals.Random.NextDouble() <= MutationChancePerNode)
                {
                    Node.Bias = Globals.Map(Globals.Random.NextDouble(), 0, 1, -1, 1);
                }
            }
        }
        public static void MutateConnections(double MutationChancePerConnection, IEnumerable<Node> Nodes, IEnumerable<Connection> Connections)
        {
            List<Node> NodesList = new List<Node>(Nodes);

            List<Node> PossibleSourceNodes = GetPossibleSourceNodes(NodesList).ToList();
            List<Node> PossibleTargetNodes = GetPossibleTargetNodes(NodesList).ToList();

            foreach (var Connection in Connections)
            {
                if (Globals.Random.NextDouble() <= MutationChancePerConnection)
                {
                    int RandomNum = Globals.Random.Next(3);

                    if (RandomNum == 0 || RandomNum == 2)
                    {
                        Connection.SourceId = GetNodeId(PossibleSourceNodes[Globals.Random.Next(PossibleSourceNodes.Count)], NodesList);
                    }

                    if (RandomNum == 1 || RandomNum == 2)
                    {
                        Connection.TargetId = GetNodeId(PossibleTargetNodes[Globals.Random.Next(PossibleTargetNodes.Count)], NodesList);
                    }
                }
            }
        }
        public static void MutateConnectionWeights(double MutationChancePerConnection, IEnumerable<Connection> Connections, double ConnectionWeightBound)
        {
            foreach (var Connection in Connections)
            {
                if (Globals.Random.NextDouble() <= MutationChancePerConnection)
                {
                    Connection.Weight = Globals.Map(Globals.Random.NextDouble(), 0, 1, -ConnectionWeightBound, ConnectionWeightBound);
                }
            }
        }
        public int GetNodeId(Node Node)
        {
            return GetNodeId(Node, Nodes);
        }
        public static int GetNodeId(Node Node, List<Node> Nodes)
        {
            return Nodes.IndexOf(Node);
        }
        public static IEnumerable<Node> GetPossibleSourceNodes(IEnumerable<Node> Nodes)
        {
            return Nodes.Where(x => x.NodeType == NodeType.Input || x.NodeType == NodeType.Process);
        }
        public static IEnumerable<Node> GetPossibleTargetNodes(IEnumerable<Node> Nodes)
        {
            return Nodes.Where(x => x.NodeType == NodeType.Process || x.NodeType == NodeType.Output);
        }
        #endregion
    }
}
