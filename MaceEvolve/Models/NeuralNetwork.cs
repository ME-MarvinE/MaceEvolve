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
        private Dictionary<int, Node> _NodeIdsToNodesDict = new Dictionary<int, Node>();
        private Dictionary<Node, int> _NodesToNodeIdsDict = new Dictionary<Node, int>();
        #endregion

        #region Properties
        public ReadOnlyDictionary<CreatureInput, double> InputValues { get; }
        public List<CreatureAction> Actions { get; } = new List<CreatureAction>();
        public List<Connection> Connections { get; set; } = new List<Connection>();
        public List<NeuralNetworkStepInfo> PreviousStepInfo { get; set; } = new List<NeuralNetworkStepInfo>();

        public IReadOnlyDictionary<int, Node> NodeIdsToNodesDict { get; } = new Dictionary<int, Node>();
        public IReadOnlyDictionary<Node, int> NodesToNodeIdsDict { get; } = new Dictionary<Node, int>();
        #endregion

        #region Constructors
        public NeuralNetwork(List<CreatureInput> Inputs, int MaxProcessNodes, List<CreatureAction> Actions)
            : this(GenerateInputNodes(Inputs).Concat(GenerateProcessNodes(MaxProcessNodes)).Concat(GenerateOutputNodes(Actions)).ToList(), Inputs, Actions, new List<Connection>())
        {
        }
        public NeuralNetwork(List<CreatureInput> Inputs, int MaxProcessNodes, List<CreatureAction> Actions, List<Connection> Connections)
            : this(GenerateInputNodes(Inputs).Concat(GenerateProcessNodes(MaxProcessNodes)).Concat(GenerateOutputNodes(Actions)).ToList(), Inputs, Actions, Connections)
        {
        }
        public NeuralNetwork(List<Node> Nodes, List<CreatureInput> Inputs, List<CreatureAction> Actions, List<Connection> Connections)
        {
            if (Inputs == null) { throw new ArgumentNullException(nameof(Inputs)); }
            if (Nodes == null) { throw new ArgumentNullException(nameof(Nodes)); }
            if (Actions == null) { throw new ArgumentNullException(nameof(Nodes)); }
            if (Connections == null) { throw new ArgumentNullException(nameof(Nodes)); }

            NodeIdsToNodesDict = new ReadOnlyDictionary<int, Node>(_NodeIdsToNodesDict);
            NodesToNodeIdsDict = new ReadOnlyDictionary<Node, int>(_NodesToNodeIdsDict);

            this.Actions = Actions;
            this.Connections = Connections;
            _InputValues = Inputs.ToDictionary(x => x, x => 0d);
            InputValues = new ReadOnlyDictionary<CreatureInput, double>(_InputValues);

            foreach (var Node in Nodes)
            {
                AddNode(Node);
            }
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
        public List<Connection> GenerateRandomConnections(int MinConnections, int MaxConnections, double WeightBound)
        {
            List<Connection> GeneratedConnections = new List<Connection>();
            int TargetConnectionAmount = Globals.Random.Next(MinConnections, MaxConnections + 1);

            Dictionary<int, Node> PossibleSourceNodes = GetPossibleSourceNodes(NodeIdsToNodesDict.Values).ToDictionary(x => NodesToNodeIdsDict[x], x => x);
            Dictionary<int, Node> PossibleTargetNodes = GetPossibleTargetNodes(NodeIdsToNodesDict.Values).ToDictionary(x => NodesToNodeIdsDict[x], x => x);

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
        public bool MutateConnectionTarget(double MutationChance, Connection Connection)
        {
            List<Node> PossibleTargetNodes = GetPossibleTargetNodes(NodeIdsToNodesDict.Values).ToList();

            if (PossibleTargetNodes.Count > 0 && Globals.Random.NextDouble() <= MutationChance)
            {
                int RandomNodeNum = Globals.Random.Next(PossibleTargetNodes.Count);
                Node RandomNode = PossibleTargetNodes[RandomNodeNum];

                Connection.TargetId = NodesToNodeIdsDict[RandomNode];

                return true;
            }

            return false;
        }
        public bool MutateConnectionSource(double MutationChance, Connection Connection)
        {
            List<Node> PossibleSourceNodes = GetPossibleSourceNodes(NodeIdsToNodesDict.Values).ToList();

            if (PossibleSourceNodes.Count > 0 && Globals.Random.NextDouble() <= MutationChance)
            {
                int RandomNodeNum = Globals.Random.Next(PossibleSourceNodes.Count);
                Node RandomNode = PossibleSourceNodes[RandomNodeNum];

                Connection.SourceId = NodesToNodeIdsDict[RandomNode];

                return true;
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
        public List<List<Connection>> GetConnectionStructure()
        {
            List<List<Connection>> ConnectionPaths = new List<List<Connection>>();

            foreach (var Connection in Connections)
            {
                Node TargetNode = NodeIdsToNodesDict[Connection.TargetId];

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
        public Dictionary<int, double> Step(bool OutputNodesOnly, bool AlwaysReevaluateNodesWithSelfReferencingConnections)
        {
            if (OutputNodesOnly)
            {
                Dictionary<int, double> CachedNodeOutputs = new Dictionary<int, double>();
                List<int> InputNodeIds = new List<int>();
                List<int> OutputNodeIds = new List<int>();

                foreach (var NodeIdToNodeKeyValuePair in NodeIdsToNodesDict)
                {
                    int NodeId = NodeIdToNodeKeyValuePair.Key;
                    Node Node = NodeIdToNodeKeyValuePair.Value;

                    if (Node.NodeType == NodeType.Input)
                    {
                        InputNodeIds.Add(NodeId);
                    }
                    else if (Node.NodeType == NodeType.Output)
                    {
                        OutputNodeIds.Add(NodeId);
                    }
                }

                List<int> NodesBeingEvaluated = new List<int>();
                List<int> NodeQueue = new List<int>();

                NodeQueue.AddRange(OutputNodeIds);
                NodeQueue.AddRange(InputNodeIds);

                while (NodeQueue.Count > 0)
                {
                    int CurrentNodeId = NodeQueue[NodeQueue.Count - 1];
                    Node CurrentNode = NodeIdsToNodesDict[CurrentNodeId];
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
                                Node ConnectionSourceNode = NodeIdsToNodesDict[Connection.SourceId];

                                //If the source node is already being evaluated, meaning either the current connection is a circular reference or the source node is present earlier in the queue and is a circular reference,
                                //The cached output of the source node must be used. If there is no cached value, initialise one with a value of 0.
                                //OR
                                //Whether the node is evaluated or not, if there is a self referencing connection, use the specified parameter to determine whether it should be evaluated again or not.
                                //This is important because after a self referencing node's output is calculated, it is cached. When getting the value of that node again, something needs to decided whether to use the original output
                                //or to calculate a new output using the cached output to resolve the circular reference instead of the initial value of 0.
                                if (NodesBeingEvaluated.Contains(Connection.SourceId) || !(CurrentNodeHasSelfReferencingConnections && !AlwaysReevaluateNodesWithSelfReferencingConnections))
                                {
                                    if (CachedNodeOutputs.TryGetValue(Connection.SourceId, out double CachedSourceNodeOutput))
                                    {
                                        SourceNodeOutput = CachedSourceNodeOutput;
                                    }
                                    else
                                    {
                                        CachedNodeOutputs[Connection.SourceId] = 0;
                                        SourceNodeOutput = CachedNodeOutputs[Connection.SourceId];
                                    }
                                }
                                else
                                {
                                    NodeQueue.Add(Connection.SourceId);
                                    CurrentNodeWeightedSum = null;
                                    break;
                                }

                                CurrentNodeWeightedSum += SourceNodeOutput * Connection.Weight;
                            }
                        }
                    }

                    if (CurrentNodeWeightedSum != null)
                    {
                        double CurrentNodeOutput = CurrentNode.NodeType == NodeType.Input ? CurrentNodeWeightedSum.Value : Globals.ReLU(CurrentNodeWeightedSum.Value + CurrentNode.Bias);

                        NodesBeingEvaluated.Remove(CurrentNodeId);

                        CachedNodeOutputs[CurrentNodeId] = CurrentNodeOutput;

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
        public Dictionary<int, double> LoggedStep(bool OutputNodesOnly, bool AlwaysReevaluateNodesWithSelfReferencingConnections)
        {
            //StepInfo
            PreviousStepInfo.Clear();

            foreach (var NodeIdToNodeKeyValuePair in NodeIdsToNodesDict)
            {
                int NodeId = NodeIdToNodeKeyValuePair.Key;
                Node Node = NodeIdToNodeKeyValuePair.Value;

                PreviousStepInfo.Add(new NeuralNetworkStepInfo()
                {
                    NodeId = NodeId,
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
                List<int> InputNodeIds = new List<int>();
                List<int> OutputNodeIds = new List<int>();

                foreach (var NodeIdToNodeKeyValuePair in NodeIdsToNodesDict)
                {
                    int NodeId = NodeIdToNodeKeyValuePair.Key;
                    Node Node = NodeIdToNodeKeyValuePair.Value;

                    if (Node.NodeType == NodeType.Input)
                    {
                        InputNodeIds.Add(NodeId);
                    }
                    else if (Node.NodeType == NodeType.Output)
                    {
                        OutputNodeIds.Add(NodeId);
                    }
                }

                List<int> NodesBeingEvaluated = new List<int>();
                List<int> NodeQueue = new List<int>();

                NodeQueue.AddRange(OutputNodeIds);
                NodeQueue.AddRange(InputNodeIds);

                while (NodeQueue.Count > 0)
                {
                    int CurrentNodeId = NodeQueue[NodeQueue.Count - 1];
                    Node CurrentNode = NodeIdsToNodesDict[CurrentNodeId];
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
                                Node ConnectionSourceNode = NodeIdsToNodesDict[Connection.SourceId];

                                //If the source node is already being evaluated, meaning either the current connection is a circular reference or the source node is present earlier in the queue and is a circular reference,
                                //The cached output of the source node must be used. If there is no cached value, initialise one with a value of 0.
                                //OR
                                //Whether the node is evaluated or not, if there is a self referencing connection, use the specified parameter to determine whether it should be evaluated again or not.
                                //This is important because after a self referencing node's output is calculated, it is cached. When getting the value of that node again, something needs to decided whether to use the original output
                                //or to calculate a new output using the cached output to resolve the circular reference instead of the initial value of 0.
                                if (NodesBeingEvaluated.Contains(Connection.SourceId) || !(CurrentNodeHasSelfReferencingConnections && !AlwaysReevaluateNodesWithSelfReferencingConnections))
                                {
                                    if (CachedNodeOutputs.TryGetValue(Connection.SourceId, out double CachedSourceNodeOutput))
                                    {
                                        SourceNodeOutput = CachedSourceNodeOutput;
                                    }
                                    else
                                    {
                                        CachedNodeOutputs[Connection.SourceId] = 0;
                                        SourceNodeOutput = CachedNodeOutputs[Connection.SourceId];
                                    }
                                }
                                else
                                {
                                    NodeQueue.Add(Connection.SourceId);
                                    CurrentNodeWeightedSum = null;
                                    break;
                                }

                                CurrentNodeWeightedSum += SourceNodeOutput * Connection.Weight;
                            }
                        }
                    }

                    if (CurrentNodeWeightedSum != null)
                    {
                        double CurrentNodeOutput = CurrentNode.NodeType == NodeType.Input ? CurrentNodeWeightedSum.Value : Globals.ReLU(CurrentNodeWeightedSum.Value + CurrentNode.Bias);

                        NodesBeingEvaluated.Remove(CurrentNodeId);

                        CachedNodeOutputs[CurrentNodeId] = CurrentNodeOutput;
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
        public int AddNode(Node Node)
        {
            bool NodeIdCreated = false;
            int NodeId = -1;

            for (int i = 0; !NodeIdCreated; i++)
            {
                if (!NodeIdsToNodesDict.ContainsKey(i))
                {
                    NodeId = i;
                    NodeIdCreated = true;
            }
            }

            _NodeIdsToNodesDict[NodeId] = Node;
            _NodesToNodeIdsDict[Node] = NodeId;

            return NodeId;
        }
        public bool RemoveNode(int NodeId, bool RemoveConnections)
        {
            bool NodeWasRemoved;

            if (NodeIdsToNodesDict.TryGetValue(NodeId, out Node Node))
            {
                if (RemoveConnections)
                {
                    RemoveConnectionsToNode(NodeId);
                }

                _NodeIdsToNodesDict.Remove(NodeId);
                _NodesToNodeIdsDict.Remove(Node);

                NodeWasRemoved = true;
            }
            else
            {
                NodeWasRemoved = false;
            }

            return NodeWasRemoved;
        }
        public void RemoveConnectionsToNode(int NodeId)
        {
            foreach (var Connection in Connections)
            {
                if (Connection.SourceId == NodeId || Connection.TargetId == NodeId)
                {
                    Connections.Remove(Connection);
                }
            }
        }
        public void RemoveConnectionsToNodes(IEnumerable<int> NodeIds)
        {
            foreach (var Connection in Connections)
            {
                if (NodeIds.Any(x => x == Connection.SourceId || x == Connection.TargetId))
                {
                    Connections.Remove(Connection);
                }
            }
        }
        #endregion
    }
}