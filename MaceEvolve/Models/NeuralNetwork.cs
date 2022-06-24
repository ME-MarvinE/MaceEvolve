using MaceEvolve.Enums;
using MaceEvolve.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaceEvolve.Models
{
    public class NeuralNetwork
    {
        #region Properties
        public List<CreatureInput> InputTypes { get; } = new List<CreatureInput>();
        public Dictionary<CreatureInput, double> InputValues { get; } = new Dictionary<CreatureInput, double>();
        public List<CreatureAction> Actions { get; } = new List<CreatureAction>();
        public List<InputNode> InputNodes { get; } = new List<InputNode>();
        public List<ProcessNode> ProcessNodes { get; } = new List<ProcessNode>();
        public List<OutputNode> OutputNodes { get; } = new List<OutputNode>();
        public List<Node> Nodes { get; } = new List<Node>();
        public List<Connection> Connections { get; set; } = new List<Connection>();
        #endregion

        #region Constructors
        public NeuralNetwork(IEnumerable<CreatureInput> Inputs, int MaxProcessNodes, IEnumerable<CreatureAction> Actions)
            : this(GenerateInputNodes(Inputs), GenerateProcessNodes(MaxProcessNodes), GenerateOutputNodes(Actions))
        {
        }
        public NeuralNetwork(IEnumerable<CreatureInput> Inputs, int MaxProcessNodes, IEnumerable<CreatureAction> Actions, IEnumerable<Connection> Connections)
            : this(GenerateInputNodes(Inputs), GenerateProcessNodes(MaxProcessNodes), GenerateOutputNodes(Actions), Connections)
        {
        }
        public NeuralNetwork(IEnumerable<InputNode> InputNodes, IEnumerable<ProcessNode> ProcessNodes, IEnumerable<OutputNode> OutputNodes)
            : this(InputNodes, ProcessNodes, OutputNodes, InputNodes.Select(x => x.CreatureInput), OutputNodes.Select(x => x.CreatureAction))
        {
        }
        public NeuralNetwork(IEnumerable<InputNode> InputNodes, IEnumerable<ProcessNode> ProcessNodes, IEnumerable<OutputNode> OutputNodes, IEnumerable<Connection> Connections)
            : this(InputNodes, ProcessNodes, OutputNodes, InputNodes.Select(x => x.CreatureInput), OutputNodes.Select(x => x.CreatureAction), Connections)
        {
        }
        public NeuralNetwork(IEnumerable<InputNode> InputNodes, IEnumerable<ProcessNode> ProcessNodes, IEnumerable<OutputNode> OutputNodes, IEnumerable<CreatureInput> Inputs, IEnumerable<CreatureAction> Actions)
            : this(InputNodes, ProcessNodes, OutputNodes, Inputs, Actions, GenerateRandomConnections(2, 5, new List<Node>().Concat(InputNodes).Concat(ProcessNodes).Concat(OutputNodes)))
        {
        }
        public NeuralNetwork(IEnumerable<InputNode> InputNodes, IEnumerable<ProcessNode> ProcessNodes, IEnumerable<OutputNode> OutputNodes, IEnumerable<CreatureInput> Inputs, IEnumerable<CreatureAction> Actions, IEnumerable<Connection> Connections)
        {
            this.InputTypes = new List<CreatureInput>(Inputs);
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
        public static List<Connection> GenerateRandomConnections(int MinConnections, int MaxConnections, IEnumerable<Node> Nodes)
        {
            List<Connection> GeneratedConnections = new List<Connection>();
            int TargetConnectionAmount = Globals.Random.Next(MinConnections, MaxConnections + 1);

            List<Node> NodesList = new List<Node>(Nodes);

            Dictionary<int, Node> PossibleSourceNodes = NodesList.Where(x => x.NodeType == NodeType.Input || x.NodeType == NodeType.Process).ToDictionary(x => NodesList.IndexOf(x), x => x);
            Dictionary<int, Node> PossibleTargetNodes = NodesList.Where(x => x.NodeType == NodeType.Output || x.NodeType == NodeType.Process).ToDictionary(x => NodesList.IndexOf(x), x => x);

            if (PossibleSourceNodes.Count == 0) { throw new InvalidOperationException("No possible source nodes."); }
            if (PossibleTargetNodes.Count == 0) { throw new InvalidOperationException("No possible target nodes."); }

            while (GeneratedConnections.Count < TargetConnectionAmount)
            {
                int RandomConnectionSource = PossibleSourceNodes.Keys.ToList()[Globals.Random.Next(0, PossibleSourceNodes.Count)];
                int RandomConnectionTarget = PossibleTargetNodes.Keys.ToList()[Globals.Random.Next(0, PossibleTargetNodes.Count)];

                Connection NewConnection = new Connection() { SourceId = RandomConnectionSource, TargetId = RandomConnectionTarget, Weight = Globals.Random.NextDouble(-1, 1) };
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
        #endregion
    }
}
