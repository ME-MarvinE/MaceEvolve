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
        public Dictionary<int, CreatureValue> Inputs { get; } = new Dictionary<int, CreatureValue>();
        public Dictionary<CreatureValue, double> InputValues { get; } = new Dictionary<CreatureValue, double>();
        public List<int> EvaluatedNodeIds { get; } = new List<int>();
        public Dictionary<int, CreatureAction> Actions { get; } = new Dictionary<int, CreatureAction>();
        public Dictionary<int, Node> Nodes { get; } = new Dictionary<int, Node>();
        public List<Connection> Connections { get; set; } = new List<Connection>();
        public int TimesStepped { get; private set; }
        #endregion

        #region Constructors
        public NeuralNetwork(Dictionary<int, CreatureValue> Inputs, Dictionary<int, CreatureAction> Actions, int MaxProcessNodes)
        {
            this.Inputs = new Dictionary<int, CreatureValue>(Inputs);
            this.Actions = new Dictionary<int, CreatureAction>(Actions);
            this.MaxProcessNodes = MaxProcessNodes;

            List<Node> NewNodes = new List<Node>();
            NewNodes.AddRange(GenerateInputNodes(Inputs.Values.ToList()).Values);
            NewNodes.AddRange(GenerateOutputNodes(Actions.Values.ToList()).Values);
            NewNodes.AddRange(GenerateProcessNodes(MaxProcessNodes).Values);

            for (int i = 0; i < NewNodes.Count; i++)
            {
                Nodes.Add(i, NewNodes[i]);
            }

            foreach (var Input in Inputs)
            {
                InputValues.Add(Input.Value, 0);
            }
        }
        #endregion

        #region Methods
        public static List<Connection> GenerateRandomConnections(int MinConnections, int MaxConnections, Dictionary<int, Node> Nodes)
        {
            List<Connection> GeneratedConnections = new List<Connection>();
            int TargetConnectionAmount = Globals.Random.Next(MinConnections, MaxConnections + 1);

            Dictionary<int, Node> PossibleSourceNodes = Nodes.Where(x => x.Value.NodeType == NodeType.Input || x.Value.NodeType == NodeType.Process).ToDictionary(x => x.Key, x => x.Value);
            Dictionary<int, Node> PossibleTargetNodes = Nodes.Where(x => x.Value.NodeType == NodeType.Output || x.Value.NodeType == NodeType.Process).ToDictionary(x => x.Key, x => x.Value);

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
        public static Dictionary<int, Node> GenerateInputNodes(List<CreatureValue> PossibleInputs)
        {
            Dictionary<int, Node> InputNodes = new Dictionary<int, Node>();

            for (int i = 0; i < PossibleInputs.Count; i++)
            {
                CreatureValue CreatureValue = PossibleInputs[i];
                InputNodes.Add(i + 1, new Node(CreatureValue, Globals.Random.NextDouble(-1, 1)));
            }

            return InputNodes;
        }
        public static Dictionary<int, Node> GenerateOutputNodes(List<CreatureAction> PossibleOutputs)
        {
            Dictionary<int, Node> OutputNodes = new Dictionary<int, Node>();

            for (int i = 0; i < PossibleOutputs.Count; i++)
            {
                CreatureAction CreatureAction = PossibleOutputs[i];
                OutputNodes.Add(i + 1, new Node(CreatureAction, Globals.Random.NextDouble(-1, 1)));
            }

            return OutputNodes;
        }
        public static Dictionary<int, Node> GenerateProcessNodes(int MaxProcessNodes)
        {
            Dictionary<int, Node> ProcessNodes = new Dictionary<int, Node>();

            for (int i = 0; i < MaxProcessNodes; i++)
            {
                ProcessNodes.Add(i + 1, new Node(Globals.Random.NextDouble(-1, 1)));
            }

            return ProcessNodes;
        }
        public void StepTime()
        {
            foreach (var Key in InputValues.Keys)
            {
                InputValues[Key] = Globals.Random.NextDouble();
            }

            foreach (var OutputNode in Nodes.Values.Where(x => x.NodeType == NodeType.Output))
            {
                OutputNode.EvaluateValue(this);
            }

            TimesStepped += 1;
        }
        public int GetNodeId(Node Node)
        {
            return Nodes.First(x => x.Value == Node).Key;
        }
        public int GetCreatureValueId(CreatureValue CreatureValue)
        {
            return Inputs.First(x => x.Value == CreatureValue).Key;
        }
        public int GetCreatureActionId(CreatureAction CreatureAction)
        {
            return Actions.First(x => x.Value == CreatureAction).Key;
        }
        #endregion
    }
}
