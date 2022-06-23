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
        public int MaxProcessNodes { get; }
        public List<CreatureInput> InputTypes { get; } = new List<CreatureInput>();
        public Dictionary<CreatureInput, double> InputValues { get; } = new Dictionary<CreatureInput, double>();
        public List<CreatureAction> Actions { get; } = new List<CreatureAction>();
        public List<Node> Nodes { get; } = new List<Node>();
        public List<Connection> Connections { get; set; } = new List<Connection>();
        public int TimesStepped { get; private set; }
        #endregion

        #region Constructors
        public NeuralNetwork(IEnumerable<CreatureInput> InputTypes, IEnumerable<CreatureAction> Actions, int MaxProcessNodes)
        {
            this.InputTypes = new List<CreatureInput>(InputTypes);
            this.Actions = new List<CreatureAction>(Actions);
            this.MaxProcessNodes = MaxProcessNodes;

            Nodes.AddRange(GenerateInputNodes(this.InputTypes));
            Nodes.AddRange(GenerateOutputNodes(this.Actions));
            Nodes.AddRange(GenerateProcessNodes(MaxProcessNodes));

            foreach (var Input in InputTypes)
            {
                InputValues.Add(Input, 0);
            }
        }
        #endregion

        #region Methods
        public static List<Connection> GenerateRandomConnections(int MinConnections, int MaxConnections, IList<Node> Nodes)
        {
            List<Connection> GeneratedConnections = new List<Connection>();
            int TargetConnectionAmount = Globals.Random.Next(MinConnections, MaxConnections + 1);

            Dictionary<int, Node> PossibleSourceNodes = Nodes.Where(x => x.NodeType == NodeType.Input || x.NodeType == NodeType.Process).ToDictionary(x => Nodes.IndexOf(x), x => x);
            Dictionary<int, Node> PossibleTargetNodes = Nodes.Where(x => x.NodeType == NodeType.Output || x.NodeType == NodeType.Process).ToDictionary(x => Nodes.IndexOf(x), x => x);

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
        public static List<Node> GenerateInputNodes(IEnumerable<CreatureInput> PossibleInputs)
        {
            return PossibleInputs.Select(x => new Node(x, Globals.Random.NextDouble(-1, 1))).ToList();
        }
        public static List<Node> GenerateOutputNodes(IEnumerable<CreatureAction> PossibleOutputs)
        {
            return PossibleOutputs.Select(x => new Node(x, Globals.Random.NextDouble(-1, 1))).ToList();
        }
        public static List<Node> GenerateProcessNodes(int MaxProcessNodes)
        {
            List<Node> ProcessNodes = new List<Node>();

            for (int i = 0; i < MaxProcessNodes; i++)
            {
                ProcessNodes.Add(new Node(Globals.Random.NextDouble(-1, 1)));
            }

            return ProcessNodes;
        }
        public void StepTime()
        {
            foreach (var Key in InputValues.Keys)
            {
                InputValues[Key] = Globals.Random.NextDouble();
            }

            foreach (var OutputNode in Nodes.Where(x => x.NodeType == NodeType.Output))
            {
                OutputNode.EvaluateValue(this);
            }

            TimesStepped += 1;
        }
        #endregion
    }
}
