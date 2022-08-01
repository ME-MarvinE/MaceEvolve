using MaceEvolve.Enums;
using MaceEvolve.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MaceEvolve.Models
{
    public class NeuralNetwork
    {
        #region Fields
        private Dictionary<CreatureInput, double> _InputValues = new Dictionary<CreatureInput, double>();
        #endregion

        #region Properties
        public ReadOnlyDictionary<CreatureInput, double> InputValues { get; }
        public List<CreatureAction> Actions { get; } = new List<CreatureAction>();
        [JsonIgnore]
        public List<Node> Nodes { get; } = new List<Node>();
        public List<Connection> Connections { get; set; } = new List<Connection>();
        #endregion

        #region Constructors
        public NeuralNetwork(List<CreatureInput> Inputs, int MaxProcessNodes, List<CreatureAction> Actions, int MinConnections, int MaxConnections, double ConnectionWeightBound)
            : this(GenerateInputNodes(Inputs).Concat(GenerateProcessNodes(MaxProcessNodes)).Concat(GenerateOutputNodes(Actions)).ToList(), Inputs, Actions, MinConnections, MaxConnections, ConnectionWeightBound)
        {
        }
        public NeuralNetwork(List<CreatureInput> Inputs, int MaxProcessNodes, List<CreatureAction> Actions, List<Connection> Connections)
            : this(GenerateInputNodes(Inputs).Concat(GenerateProcessNodes(MaxProcessNodes)).Concat(GenerateOutputNodes(Actions)).ToList(), Inputs, Actions, Connections)
        {
        }
        //public NeuralNetwork(IEnumerable<Node> Nodes, int MinConnections, int MaxConnections, double ConnectionWeightBound)
        //    : this(Nodes, Nodes.Where(x => x.NodeType == NodeType.Input).Select(x => x.CreatureInput.Value).ToList(), Nodes.Where(x => x.NodeType == NodeType.Output).Select(x => x.CreatureAction.Value).ToList(), MinConnections, MaxConnections, ConnectionWeightBound)
        //{
        //}
        //public NeuralNetwork(IEnumerable<Node> Nodes, List<Connection> Connections)
        //    : this(Nodes, Nodes.Where(x => x.NodeType == NodeType.Input).Select(x => x.CreatureInput.Value).ToList(), Nodes.Where(x => x.NodeType == NodeType.Output).Select(x => x.CreatureAction.Value).ToList(), Connections)
        //{
        //}
        public NeuralNetwork(List<Node> Nodes, List<CreatureInput> Inputs, List<CreatureAction> Actions, int MinConnections, int MaxConnections, double ConnectionWeightBound)
            : this(Nodes, Inputs, Actions, GenerateRandomConnections(MinConnections, MaxConnections, Nodes, ConnectionWeightBound))
        {
        }
        [JsonConstructor]
        public NeuralNetwork(List<Node> Nodes, List<CreatureInput> Inputs, List<CreatureAction> Actions, List<Connection> Connections)
        {
            if (Inputs == null) { throw new ArgumentNullException(nameof(Inputs)); }
            this.Actions = Actions ?? throw new ArgumentNullException(nameof(Actions));
            this.Connections = Connections ?? throw new ArgumentNullException(nameof(Connections));
            this.Nodes = Nodes ?? throw new ArgumentNullException(nameof(Nodes));

            _InputValues = Inputs.ToDictionary(x => x, x => 0d);
            InputValues = new ReadOnlyDictionary<CreatureInput, double>(_InputValues);
        }
        #endregion

        #region Methods
        public void UpdateInputValue(CreatureInput CreatureInput, double Value)
        {
            if (InputValues.ContainsKey(CreatureInput))
            {
                _InputValues[CreatureInput] = Value;
            }
            else
            {
                _InputValues.Add(CreatureInput, Value);
            }
            
        }
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
        public static List<Node> GenerateInputNodes(IEnumerable<CreatureInput> PossibleInputs)
        {
            return PossibleInputs.Select(x => new Node(NodeType.Input, Globals.Random.NextDouble(-1, 1), CreatureInput: x)).ToList();
        }
        public static List<Node> GenerateOutputNodes(IEnumerable<CreatureAction> PossibleOutputs)
        {
            return PossibleOutputs.Select(x => new Node(NodeType.Output, Globals.Random.NextDouble(-1, 1), CreatureAction: x)).ToList();
        }
        public static List<Node> GenerateProcessNodes(int MaxProcessNodes)
        {
            List<Node> ProcessNodes = new List<Node>();

            for (int i = 0; i < MaxProcessNodes; i++)
            {
                ProcessNodes.Add(new Node(NodeType.Process, Globals.Random.NextDouble(-1, 1)));
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
        public static void MutateInputNodeCount(double MutationChancePerNode, IList<Node> Nodes, int MaxInputNodes, IEnumerable<CreatureInput> PossibleInputs)
        {

            List<Node> InputNodes = Nodes.Where(x => x.NodeType == NodeType.Input).ToList();
            int MaxInputNodesToAdd = MaxInputNodes - InputNodes.Count;
            List<CreatureInput> PossibleInputsToAdd = PossibleInputs.Where(x => !InputNodes.Any(y => y.CreatureInput == x)).ToList();


            if (PossibleInputsToAdd.Count > 0)
            {
                for (int i = 0; i < MaxInputNodesToAdd; i++)
                {
                    if (Globals.Random.NextDouble() <= MutationChancePerNode)
                    {
                        CreatureInput RandomPossibleInput = PossibleInputsToAdd[Globals.Random.Next(PossibleInputsToAdd.Count)];
                        Nodes.Add(new Node(NodeType.Input, Globals.Map(Globals.Random.NextDouble(), 0, 1, -1, 1), CreatureInput: RandomPossibleInput));
                    }
                }
            }
        }
        public static void MutateProcessNodeCount(double MutationChancePerNode, IList<Node> Nodes, int MaxProcessNodes)
        {
            List<Node> ProcessNodes = Nodes.Where(x => x.NodeType == NodeType.Process).ToList();
            int MaxProcessNodesToAdd = MaxProcessNodes - ProcessNodes.Count;

            for (int i = 0; i < MaxProcessNodesToAdd; i++)
            {
                if (Globals.Random.NextDouble() <= MutationChancePerNode)
                {
                    Nodes.Add(new Node(NodeType.Process, Globals.Map(Globals.Random.NextDouble(), 0, 1, -1, 1)));
                }
            }
        }
        public static void MutateOutputNodeCount(double MutationChancePerNode, IList<Node> Nodes, int MaxOutputNodes, IEnumerable<CreatureAction> PossibleActions)
        {

            List<Node> OutputNodes = Nodes.Where(x => x.NodeType == NodeType.Output).ToList();
            int MaxOutputNodesToAdd = MaxOutputNodes - OutputNodes.Count;
            List<CreatureAction> PossibleOutputsToAdd = PossibleActions.Where(x => !OutputNodes.Any(y => y.CreatureAction == x)).ToList();


            if (PossibleOutputsToAdd.Count > 0)
            {
                for (int i = 0; i < MaxOutputNodesToAdd; i++)
                {
                    if (Globals.Random.NextDouble() <= MutationChancePerNode)
                    {
                        CreatureAction RandomPossibleAction = PossibleOutputsToAdd[Globals.Random.Next(PossibleOutputsToAdd.Count)];
                        Nodes.Add(new Node(NodeType.Output, Globals.Map(Globals.Random.NextDouble(), 0, 1, -1, 1), CreatureAction: RandomPossibleAction));
                    }
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

                    if (PossibleSourceNodes.Count > 0 && (RandomNum == 0 || RandomNum == 2))
                    {
                        int RandomNodeNum = Globals.Random.Next(PossibleSourceNodes.Count);
                        Node RandomNode = PossibleSourceNodes[RandomNodeNum];

                        Connection.SourceId = GetNodeId(RandomNode, NodesList);
                    }

                    if (PossibleTargetNodes.Count > 0 && (RandomNum == 1 || RandomNum == 2))
                    {
                        int RandomNodeNum = Globals.Random.Next(PossibleTargetNodes.Count);
                        Node RandomNode = PossibleTargetNodes[RandomNodeNum];

                        Connection.TargetId = GetNodeId(RandomNode, NodesList);
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
