using MaceEvolve.Enums;
using MaceEvolve.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;

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

        public Dictionary<Node, double> PreviousNodeOutputs { get; set; } = new Dictionary<Node, double>();
        public List<NeuralNetworkStepInfo> PreviousStepInfo { get; set; } = new List<NeuralNetworkStepInfo>();
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
        public static bool MutateNodeBias(double MutationChance, Node Node)
        {
            if (Globals.Random.NextDouble() <= MutationChance)
            {
                Node.Bias = Globals.Map(Globals.Random.NextDouble(), 0, 1, -1, 1);

                return true;
            }

            return false;
        }
        public static Node GetInputNodeToAdd(double AddNodeChance, IList<Node> Nodes, IEnumerable<CreatureInput> PossibleInputs)
        {
            if (Globals.Random.NextDouble() <= AddNodeChance)
            {
                IEnumerable<Node> InputNodes = Nodes.Where(x => x.NodeType == NodeType.Input);
                //Possible inputs are any that aren't already present in the creature.
                List<CreatureInput> PossibleInputsToAdd = PossibleInputs.Where(x => !InputNodes.Any(y => y.CreatureInput == x)).ToList();

                if (PossibleInputsToAdd.Count > 0)
                {
                    CreatureInput RandomPossibleInput = PossibleInputsToAdd[Globals.Random.Next(PossibleInputsToAdd.Count)];
                    Node NewNode = new Node(NodeType.Input, Globals.Map(Globals.Random.NextDouble(), 0, 1, -1, 1), CreatureInput: RandomPossibleInput);
                    Nodes.Add(NewNode);

                    return NewNode;
                }
            }

            return null;
        }
        public static Node GetInputNodeToRemove(double RemoveNodeChance, IEnumerable<Node> Nodes)
        {
            if (Globals.Random.NextDouble() <= RemoveNodeChance)
            {
                List<Node> InputNodes = Nodes.Where(x => x.NodeType == NodeType.Input).ToList();

                if (InputNodes.Count > 0)
                {
                    return InputNodes[Globals.Random.Next(InputNodes.Count)];
                }
            }

            return null;
        }
        public static Node GetProcessNodeToAdd(double AddNodeChance)
        {
            if (Globals.Random.NextDouble() <= AddNodeChance)
            {
                Node NewNode = new Node(NodeType.Process, Globals.Map(Globals.Random.NextDouble(), 0, 1, -1, 1));

                return NewNode;
            }

            return null;
        }
        public static Node GetProcessNodeToRemove(double RemoveNodeChance, IEnumerable<Node> Nodes)
        {
            if (Globals.Random.NextDouble() <= RemoveNodeChance)
            {
                List<Node> ProcessNodes = Nodes.Where(x => x.NodeType == NodeType.Process).ToList();

                if (ProcessNodes.Count > 0)
                {
                    return ProcessNodes[Globals.Random.Next(ProcessNodes.Count)];
                }
            }

            return null;
        }
        public static Node GetOutputNodeToAdd(double AddNodeChance, IList<Node> Nodes, IEnumerable<CreatureAction> PossibleActions)
        {
            if (Globals.Random.NextDouble() <= AddNodeChance)
            {
                IEnumerable<Node> OutputNodes = Nodes.Where(x => x.NodeType == NodeType.Output);
                //Possible outputs are any that aren't already present in the creature.
                List<CreatureAction> PossibleActionsToAdd = PossibleActions.Where(x => !OutputNodes.Any(y => y.CreatureAction == x)).ToList();

                if (PossibleActionsToAdd.Count > 0)
                {
                    CreatureAction RandomPossibleAction = PossibleActionsToAdd[Globals.Random.Next(PossibleActionsToAdd.Count)];
                    Node NewNode = new Node(NodeType.Output, Globals.Map(Globals.Random.NextDouble(), 0, 1, -1, 1), CreatureAction: RandomPossibleAction);
                    Nodes.Add(NewNode);

                    return NewNode;
                }
            }

            return null;
        }
        public static Node GetOutputNodeToRemove(double RemoveNodeChance, IEnumerable<Node> Nodes)
        {
            if (Globals.Random.NextDouble() <= RemoveNodeChance)
            {
                List<Node> OutputNodes = Nodes.Where(x => x.NodeType == NodeType.Output).ToList();

                if (OutputNodes.Count > 0)
                {
                    return OutputNodes[Globals.Random.Next(OutputNodes.Count)];
                }
            }

            return null;
        }
        public static Node GetNodeToAdd(double InputNodeChance, double ProcessNodeChance, double OutputNodeChance, IList<Node> Nodes, IEnumerable<CreatureInput> PossibleInputs, IEnumerable<CreatureAction> PossibleActions)
        {
            double TotalChance = InputNodeChance + ProcessNodeChance + OutputNodeChance;

            if (TotalChance > 0)
            {
                if (Globals.Random.NextDouble() <= InputNodeChance / TotalChance)
                {
                    return GetInputNodeToAdd(1, Nodes, PossibleInputs);
                }
                else if (Globals.Random.NextDouble() <= ProcessNodeChance / TotalChance)
                {
                    return GetProcessNodeToAdd(1);
                }
                else if (Globals.Random.NextDouble() <= OutputNodeChance / TotalChance)
                {
                    return GetOutputNodeToAdd(1, Nodes, PossibleActions);
                }
            }

            return null;
        }
        public static Node GetNodeToRemove(double InputNodeChance, double ProcessNodeChance, double OutputNodeChance, IEnumerable<Node> Nodes)
        {
            double TotalChance = InputNodeChance + ProcessNodeChance + OutputNodeChance;

            if (TotalChance > 0)
            {
                if (Globals.Random.NextDouble() <= InputNodeChance / TotalChance)
                {
                    return GetInputNodeToRemove(1, Nodes);
                }
                else if (Globals.Random.NextDouble() <= ProcessNodeChance / TotalChance)
                {
                    return GetProcessNodeToRemove(1, Nodes);
                }
                else if (Globals.Random.NextDouble() <= OutputNodeChance / TotalChance)
                {
                    return GetOutputNodeToRemove(1, Nodes);
                }
            }

            return null;
        }
        public static bool MutateConnectionTarget(double MutationChance, IList<Node> Nodes, Connection Connection)
        {
            List<Node> PossibleTargetNodes = GetPossibleTargetNodes(Nodes).ToList();

            if (Globals.Random.NextDouble() <= MutationChance)
            {
                if (PossibleTargetNodes.Count > 0)
                {
                    int RandomNodeNum = Globals.Random.Next(PossibleTargetNodes.Count);
                    Node RandomNode = PossibleTargetNodes[RandomNodeNum];

                    Connection.TargetId = GetNodeId(RandomNode, Nodes);
                    return true;
                }
            }

            return false;
        }
        public static bool MutateConnectionSource(double MutationChance, IList<Node> Nodes, Connection Connection)
        {
            List<Node> PossibleSourceNodes = GetPossibleSourceNodes(Nodes).ToList();

            if (Globals.Random.NextDouble() <= MutationChance)
            {
                if (PossibleSourceNodes.Count > 0)
                {
                    int RandomNodeNum = Globals.Random.Next(PossibleSourceNodes.Count);
                    Node RandomNode = PossibleSourceNodes[RandomNodeNum];

                    Connection.SourceId = GetNodeId(RandomNode, Nodes);

                    return true;
                }
            }

            return false;
        }
        public static bool MutateConnectionWeight(double MutationChance, Connection Connection, double ConnectionWeightBound)
        {
            if (Globals.Random.NextDouble() <= MutationChance)
            {
                Connection.Weight = Globals.Map(Globals.Random.NextDouble(), 0, 1, -ConnectionWeightBound, ConnectionWeightBound);

                return true;
            }

            return false;
        }
        public static bool MutateConnection(Connection Connection, IList<Node> Nodes, double SourceMutationChance, double TargetMutationChance, double WeightMutationChance, double WeightBound)
        {
            return MutateConnectionSource(SourceMutationChance, Nodes, Connection) ||
                MutateConnectionTarget(TargetMutationChance, Nodes, Connection) ||
                MutateConnectionWeight(WeightMutationChance, Connection, WeightBound);
        }
        public static void RemoveConnectionsToNode(int NodeIdToRemoveConnectionsFrom, IList<Connection> Connections)
        {
            List<Connection> ConnectionsToCheck = Connections.ToList();

            foreach (var Connection in ConnectionsToCheck)
            {
                if (Connection.SourceId == NodeIdToRemoveConnectionsFrom || Connection.TargetId == NodeIdToRemoveConnectionsFrom)
                {
                    Connections.Remove(Connection);
                }
            }
        }
        public static void RemoveConnectionsToNodes(IEnumerable<int> NodeIdsToRemoveConnectionsFrom, IList<Connection> Connections)
        {
            List<Connection> ConnectionsToCheck = Connections.ToList();

            foreach (var Connection in ConnectionsToCheck)
            {
                if (NodeIdsToRemoveConnectionsFrom.Any(x => x == Connection.SourceId || x == Connection.TargetId))
                {
                    Connections.Remove(Connection);
                }
            }
        }
        public int GetNodeId(Node Node)
        {
            return GetNodeId(Node, Nodes);
        }
        public static int GetNodeId(Node Node, IList<Node> Nodes)
        {
            return Nodes.IndexOf(Node);
        }
        public void RemoveNodeAndConnections(Node Node)
        {
            int NodeId = GetNodeId(Node);

            RemoveConnectionsToNode(NodeId, Connections);

            foreach (var Connection in Connections)
            {
                if (Connection.SourceId > NodeId)
                {
                    Connection.SourceId -= 1;
                }

                if (Connection.TargetId > NodeId)
                {
                    Connection.TargetId -= 1;
                }
            }
        }
        public List<List<Connection>> GetConnectionStructure()
        {
            List<List<Connection>> ConnectionPaths = new List<List<Connection>>();

            foreach (var Connection in Connections)
            {
                Node TargetNode = Nodes[Connection.TargetId];

                if (TargetNode.NodeType == NodeType.Output)
                {
                    ConnectionPaths.Add(new List<Connection>() { Connection });
                }
            }

            for (int i = 0; i < ConnectionPaths.Count; i++)
            {
                List<Connection> ConnectionPath = ConnectionPaths[i];
                List<Connection> SourceConnections = Connections.Where(x => !ConnectionPath.Any(y => y.TargetId == x.SourceId) && x.TargetId == ConnectionPath.Last().SourceId).ToList();
                if (SourceConnections.Count > 1)
                {
                    for (int j = 1; j < SourceConnections.Count; j++)
                    {
                        List<Connection> NewConnectionStructure = ConnectionPath.ToList();
                        NewConnectionStructure.Add(SourceConnections[j]);

                        ConnectionPaths.Add(NewConnectionStructure);
                    }

                    ConnectionPath.Add(SourceConnections[0]);
                }
            }

            return ConnectionPaths;
        }
        public static IEnumerable<Node> GetPossibleSourceNodes(IEnumerable<Node> Nodes)
        {
            return Nodes.Where(x => x.NodeType == NodeType.Input || x.NodeType == NodeType.Process);
        }
        public static IEnumerable<Node> GetPossibleTargetNodes(IEnumerable<Node> Nodes)
        {
            return Nodes.Where(x => x.NodeType == NodeType.Process || x.NodeType == NodeType.Output);
        }
        public Dictionary<int, double> Step(bool OutputNodesOnly, bool CacheSelfReferencingConnections)
        {
            if (OutputNodesOnly)
            {
                Dictionary<int, double> CachedNodeOutputs = new Dictionary<int, double>();
                Dictionary<Node, int> NodeToIdDict = Nodes.ToDictionary(x => x, x => GetNodeId(x));
                Dictionary<int, Node> IdToNodeDict = NodeToIdDict.ToDictionary(x => x.Value, x => x.Key);
                List<int> InputNodeIds = new List<int>();
                List<int> OutputNodeIds = new List<int>();

                foreach (var Node in Nodes)
                {
                    if (Node.NodeType == NodeType.Input)
                    {
                        InputNodeIds.Add(NodeToIdDict[Node]);
                    }
                    else if (Node.NodeType == NodeType.Output)
                    {
                        OutputNodeIds.Add(NodeToIdDict[Node]);
                    }
                }

                List<int> NodesBeingEvaluated = new List<int>();
                List<int> NodeQueue = new List<int>();

                NodeQueue.AddRange(OutputNodeIds);
                NodeQueue.AddRange(InputNodeIds);

                while (NodeQueue.Count > 0)
                {
                    int CurrentNodeId = NodeQueue[NodeQueue.Count - 1];
                    Node CurrentNode = IdToNodeDict[CurrentNodeId];
                    NodesBeingEvaluated.Add(CurrentNodeId);
                    double? CurrentNodeWeightedSum;

                    if (CurrentNode.NodeType == NodeType.Input)
                    {
                        if (CurrentNode.CreatureInput == null)
                        {
                            throw new InvalidOperationException($"Node type is {CurrentNode.NodeType} but {nameof(CreatureInput)} is null.");
                        }

                        CurrentNodeWeightedSum = InputValues[CurrentNode.CreatureInput.Value];
                    }
                    else
                    {
                        if (CurrentNode.NodeType == NodeType.Output && CurrentNode.CreatureAction == null)
                        {
                            throw new InvalidOperationException($"Node type is {CurrentNode.NodeType} but {nameof(CreatureAction)} is null.");
                        }

                        CurrentNodeWeightedSum = 0;

                        List<Connection> ConnectionsToCurrentNode = Connections.Where(x => x.TargetId == CurrentNodeId).ToList();
                        bool CurrentNodeHasSelfReferencingConnections = ConnectionsToCurrentNode.Any(x => x.SourceId == CurrentNodeId);

                        foreach (var Connection in ConnectionsToCurrentNode)
                        {
                            if (Connection.TargetId == CurrentNodeId)
                            {
                                double SourceNodeOutput;
                                Node ConnectionSourceNode = IdToNodeDict[Connection.SourceId];

                                if (NodesBeingEvaluated.Contains(Connection.SourceId) || (CurrentNodeHasSelfReferencingConnections && !CacheSelfReferencingConnections))
                                {
                                    if (PreviousNodeOutputs.TryGetValue(ConnectionSourceNode, out double PreviousSourceNodeOutput))
                                    {
                                        SourceNodeOutput = PreviousSourceNodeOutput;
                                    }
                                    else
                                    {
                                        PreviousNodeOutputs[ConnectionSourceNode] = 0;
                                        SourceNodeOutput = PreviousNodeOutputs[ConnectionSourceNode];
                                    }
                                }
                                else
                                {
                                    if (CachedNodeOutputs.TryGetValue(Connection.SourceId, out double CachedSourceNodeOutput))
                                    {
                                        SourceNodeOutput = CachedSourceNodeOutput;
                                    }
                                    else
                                    {
                                        NodeQueue.Add(Connection.SourceId);
                                        CurrentNodeWeightedSum = null;
                                        break;
                                    }
                                }

                                //CurrentNodeWeightedSum ??= 0;

                                CurrentNodeWeightedSum += SourceNodeOutput * Connection.Weight;
                            }
                        }
                    }

                    if (CurrentNodeWeightedSum != null)
                    {
                        double CurrentNodeOutput = Globals.ReLU(CurrentNodeWeightedSum.Value + CurrentNode.Bias);

                        if (CurrentNodeOutput > 10000)
                        {
                            var thing = 2;
                        }

                        NodesBeingEvaluated.Remove(CurrentNodeId);

                        CachedNodeOutputs[CurrentNodeId] = CurrentNodeOutput;
                        PreviousNodeOutputs[CurrentNode] = CurrentNodeOutput;

                        NodeQueue.Remove(CurrentNodeId);
                    }
                }

                return CachedNodeOutputs;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public Dictionary<int, double> LoggedStep(bool OutputNodesOnly, bool CacheSelfReferencingConnections)
        {
            Dictionary<Node, int> NodeToIdDict = Nodes.ToDictionary(x => x, x => GetNodeId(x));

            //StepInfo
            PreviousStepInfo.Clear();

            foreach (var Node in Nodes)
            {
                PreviousStepInfo.Add(new NeuralNetworkStepInfo()
                {
                    NodeId = NodeToIdDict[Node],
                    Bias = Node.Bias,
                    CreatureAction = Node.CreatureAction,
                    CreatureInput = Node.CreatureInput,
                    NodeType = Node.NodeType
                });
            }

            foreach (var Connection in Connections)
            {
                NeuralNetworkStepInfo SourceStepInfo = PreviousStepInfo.First(x => x.NodeId == Connection.SourceId);
                NeuralNetworkStepInfo TargetStepInfo = PreviousStepInfo.First(x => x.NodeId == Connection.TargetId);

                if (!SourceStepInfo.ConnectionsFrom.Contains(Connection))
                {
                    SourceStepInfo.ConnectionsFrom.Add(Connection);
                }

                if (!TargetStepInfo.ConnectionsTo.Contains(Connection))
                {
                    TargetStepInfo.ConnectionsTo.Add(Connection);
                }
            }

            if (OutputNodesOnly)
            {
                Dictionary<int, double> CachedNodeOutputs = new Dictionary<int, double>();
                Dictionary<int, Node> IdToNodeDict = NodeToIdDict.ToDictionary(x => x.Value, x => x.Key);
                List<int> InputNodeIds = new List<int>();
                List<int> OutputNodeIds = new List<int>();

                foreach (var Node in Nodes)
                {
                    if (Node.NodeType == NodeType.Input)
                    {
                        InputNodeIds.Add(NodeToIdDict[Node]);
                    }
                    else if (Node.NodeType == NodeType.Output)
                    {
                        OutputNodeIds.Add(NodeToIdDict[Node]);
                    }
                }

                List<int> NodesBeingEvaluated = new List<int>();
                List<int> NodeQueue = new List<int>();

                NodeQueue.AddRange(OutputNodeIds);
                NodeQueue.AddRange(InputNodeIds);

                while (NodeQueue.Count > 0)
                {
                    int CurrentNodeId = NodeQueue[NodeQueue.Count - 1];
                    Node CurrentNode = IdToNodeDict[CurrentNodeId];
                    NodesBeingEvaluated.Add(CurrentNodeId);
                    NeuralNetworkStepInfo CurrentNodeStepInfo = PreviousStepInfo.First(x => x.NodeId == CurrentNodeId);
                    double? CurrentNodeWeightedSum;

                    if (CurrentNode.NodeType == NodeType.Input)
                    {
                        if (CurrentNode.CreatureInput == null)
                        {
                            throw new InvalidOperationException($"Node type is {CurrentNode.NodeType} but {nameof(CreatureInput)} is null.");
                        }

                        CurrentNodeWeightedSum = InputValues[CurrentNode.CreatureInput.Value];
                    }
                    else
                    {
                        if (CurrentNode.NodeType == NodeType.Output && CurrentNode.CreatureAction == null)
                        {
                            throw new InvalidOperationException($"Node type is {CurrentNode.NodeType} but {nameof(CreatureAction)} is null.");
                        }

                        CurrentNodeWeightedSum = 0;

                        List<Connection> ConnectionsToCurrentNode = Connections.Where(x => x.TargetId == CurrentNodeId).ToList();
                        bool CurrentNodeHasSelfReferencingConnections = ConnectionsToCurrentNode.Any(x => x.SourceId == CurrentNodeId);

                        foreach (var Connection in ConnectionsToCurrentNode)
                        {
                            if (Connection.TargetId == CurrentNodeId)
                            {
                                double SourceNodeOutput;
                                Node ConnectionSourceNode = IdToNodeDict[Connection.SourceId];

                                if (NodesBeingEvaluated.Contains(Connection.SourceId) || (CurrentNodeHasSelfReferencingConnections && !CacheSelfReferencingConnections))
                                {
                                    if (PreviousNodeOutputs.TryGetValue(ConnectionSourceNode, out double PreviousSourceNodeOutput))
                                    {
                                        SourceNodeOutput = PreviousSourceNodeOutput;
                                    }
                                    else
                                    {
                                        PreviousNodeOutputs[ConnectionSourceNode] = 0;
                                        SourceNodeOutput = PreviousNodeOutputs[ConnectionSourceNode];
                                    }
                                }
                                else
                                {
                                    if (CachedNodeOutputs.TryGetValue(Connection.SourceId, out double CachedSourceNodeOutput))
                                    {
                                        SourceNodeOutput = CachedSourceNodeOutput;
                                    }
                                    else
                                    {
                                        NodeQueue.Add(Connection.SourceId);
                                        CurrentNodeWeightedSum = null;
                                        break;
                                    }
                                }

                                //CurrentNodeWeightedSum ??= 0;

                                CurrentNodeWeightedSum += SourceNodeOutput * Connection.Weight;
                            }
                        }
                    }

                    if (CurrentNodeWeightedSum != null)
                    {
                        double CurrentNodeOutput = CurrentNode.NodeType == NodeType.Input ? CurrentNodeWeightedSum.Value : Globals.ReLU(CurrentNodeWeightedSum.Value + CurrentNode.Bias);

                        if (CurrentNodeOutput > 10000)
                        {
                            var thing = 2;
                        }

                        NodesBeingEvaluated.Remove(CurrentNodeId);

                        CachedNodeOutputs[CurrentNodeId] = CurrentNodeOutput;
                        PreviousNodeOutputs[CurrentNode] = CurrentNodeOutput;
                        CurrentNodeStepInfo.PreviousOutput = CurrentNodeOutput;

                        NodeQueue.Remove(CurrentNodeId);
                    }
                }

                return CachedNodeOutputs;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        #endregion
    }
}