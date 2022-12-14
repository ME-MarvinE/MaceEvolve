using MaceEvolve.Core.Enums;
using MaceEvolve.Core.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MaceEvolve.Core.Models
{
    public class NeuralNetwork
    {
        #region Fields
        private Dictionary<CreatureInput, float> _inputValues = new Dictionary<CreatureInput, float>();
        private Dictionary<int, Node> _nodeIdsToNodesDict = new Dictionary<int, Node>();
        private Dictionary<Node, int> _nodesToNodeIdsDict = new Dictionary<Node, int>();
        #endregion

        #region Properties
        public ReadOnlyDictionary<CreatureInput, float> InputValues { get; }
        public List<CreatureAction> Actions { get; } = new List<CreatureAction>();
        public List<Connection> Connections { get; set; } = new List<Connection>();
        public List<NeuralNetworkStepNodeInfo> PreviousStepInfo { get; set; } = new List<NeuralNetworkStepNodeInfo>();

        public IReadOnlyDictionary<int, Node> NodeIdsToNodesDict { get; } = new Dictionary<int, Node>();
        public IReadOnlyDictionary<Node, int> NodesToNodeIdsDict { get; } = new Dictionary<Node, int>();
        #endregion

        #region Constructors
        public NeuralNetwork(List<CreatureInput> inputs, int maxProcessNodes, List<CreatureAction> actions)
            : this(GenerateInputNodes(inputs).Concat(GenerateProcessNodes(maxProcessNodes)).Concat(GenerateOutputNodes(actions)).ToList(), inputs, actions, new List<Connection>())
        {
        }
        public NeuralNetwork(List<CreatureInput> inputs, int maxProcessNodes, List<CreatureAction> actions, List<Connection> connections)
            : this(GenerateInputNodes(inputs).Concat(GenerateProcessNodes(maxProcessNodes)).Concat(GenerateOutputNodes(actions)).ToList(), inputs, actions, connections)
        {
        }
        public NeuralNetwork(List<Node> nodes, List<CreatureInput> inputs, List<CreatureAction> actions, List<Connection> connections)
        {
            if (inputs == null) { throw new ArgumentNullException(nameof(inputs)); }
            if (nodes == null) { throw new ArgumentNullException(nameof(nodes)); }
            if (actions == null) { throw new ArgumentNullException(nameof(nodes)); }
            if (connections == null) { throw new ArgumentNullException(nameof(nodes)); }

            NodeIdsToNodesDict = new ReadOnlyDictionary<int, Node>(_nodeIdsToNodesDict);
            NodesToNodeIdsDict = new ReadOnlyDictionary<Node, int>(_nodesToNodeIdsDict);

            Actions = actions;
            Connections = connections;
            _inputValues = inputs.ToDictionary(x => x, x => 0f);
            InputValues = new ReadOnlyDictionary<CreatureInput, float>(_inputValues);

            foreach (var node in nodes)
            {
                AddNode(node);
            }
        }
        #endregion

        #region Methods
        public void UpdateInputValue(CreatureInput creatureInput, float value)
        {
            _inputValues[creatureInput] = value;
        }
        public List<Connection> GenerateRandomConnections(int minConnections, int maxConnections, float weightBound)
        {
            List<Connection> generatedConnections = new List<Connection>();
            int targetConnectionAmount = Globals.Random.Next(minConnections, maxConnections + 1);

            Dictionary<int, Node> possibleSourceNodes = GetSourceNodes(NodeIdsToNodesDict.Values).ToDictionary(x => NodesToNodeIdsDict[x], x => x);
            Dictionary<int, Node> possibleTargetNodes = GetTargetNodes(NodeIdsToNodesDict.Values).ToDictionary(x => NodesToNodeIdsDict[x], x => x);

            if (possibleSourceNodes.Count == 0) { throw new InvalidOperationException("No possible source nodes."); }
            if (possibleTargetNodes.Count == 0) { throw new InvalidOperationException("No possible target nodes."); }

            while (generatedConnections.Count < targetConnectionAmount)
            {
                int randomConnectionSource = possibleSourceNodes.Keys.ToList()[Globals.Random.Next(0, possibleSourceNodes.Count)];
                int randomConnectionTarget = possibleTargetNodes.Keys.ToList()[Globals.Random.Next(0, possibleTargetNodes.Count)];

                Connection newConnection = new Connection() { SourceId = randomConnectionSource, TargetId = randomConnectionTarget, Weight = Globals.Random.NextFloat(-weightBound, weightBound) };
                generatedConnections.Add(newConnection);
            }

            return generatedConnections;
        }
        public static List<Node> GenerateInputNodes(IEnumerable<CreatureInput> possibleInputs)
        {
            return possibleInputs.Select(x => new Node(NodeType.Input, Globals.Random.NextFloat(-1, 1), creatureInput: x)).ToList();
        }
        public static List<Node> GenerateOutputNodes(IEnumerable<CreatureAction> possibleOutputs)
        {
            return possibleOutputs.Select(x => new Node(NodeType.Output, Globals.Random.NextFloat(-1, 1), creatureAction: x)).ToList();
        }
        public static List<Node> GenerateProcessNodes(int maxProcessNodes)
        {
            List<Node> processNodes = new List<Node>();

            for (int i = 0; i < maxProcessNodes; i++)
            {
                processNodes.Add(new Node(NodeType.Process, Globals.Random.NextFloat(-1, 1)));
            }

            return processNodes;
        }
        public NeuralNetwork CloneNetwork()
        {
            return JsonConvert.DeserializeObject<NeuralNetwork>(JsonConvert.SerializeObject(this));
        }
        public bool MutateConnectionTarget(float mutationChance, Connection connection)
        {
            List<Node> possibleTargetNodes = GetTargetNodes(NodeIdsToNodesDict.Values).ToList();

            if (possibleTargetNodes.Count > 0 && Globals.Random.NextFloat() <= mutationChance)
            {
                int randomNodeNum = Globals.Random.Next(possibleTargetNodes.Count);
                Node randomNode = possibleTargetNodes[randomNodeNum];

                connection.TargetId = NodesToNodeIdsDict[randomNode];

                return true;
            }

            return false;
        }
        public bool MutateConnectionSource(float mutationChance, Connection connection)
        {
            List<Node> possibleSourceNodes = GetSourceNodes(NodeIdsToNodesDict.Values).ToList();

            if (possibleSourceNodes.Count > 0 && Globals.Random.NextFloat() <= mutationChance)
            {
                int randomNodeNum = Globals.Random.Next(possibleSourceNodes.Count);
                Node randomNode = possibleSourceNodes[randomNodeNum];

                connection.SourceId = NodesToNodeIdsDict[randomNode];

                return true;
            }

            return false;
        }
        public List<List<Connection>> GetConnectionStructure()
        {
            List<List<Connection>> connectionPaths = new List<List<Connection>>();

            foreach (var connection in Connections)
            {
                Node targetNode = NodeIdsToNodesDict[connection.TargetId];

                if (targetNode.NodeType == NodeType.Output)
                {
                    connectionPaths.Add(new List<Connection>() { connection });
                }
            }

            for (int i = 0; i < connectionPaths.Count; i++)
            {
                List<Connection> connectionPath = connectionPaths[i];
                List<Connection> sourceConnections = Connections.Where(x => !connectionPath.Any(y => y.TargetId == x.SourceId) && x.TargetId == connectionPath.Last().SourceId).ToList();
                if (sourceConnections.Count > 1)
                {
                    for (int j = 1; j < sourceConnections.Count; j++)
                    {
                        List<Connection> newConnectionStructure = connectionPath.ToList();
                        newConnectionStructure.Add(sourceConnections[j]);

                        connectionPaths.Add(newConnectionStructure);
                    }

                    connectionPath.Add(sourceConnections[0]);
                }
            }

            return connectionPaths;
        }
        public static IEnumerable<Node> GetSourceNodes(IEnumerable<Node> nodes)
        {
            return nodes.Where(x => x.NodeType == NodeType.Input || x.NodeType == NodeType.Process);
        }
        public static IEnumerable<Node> GetTargetNodes(IEnumerable<Node> nodes)
        {
            return nodes.Where(x => x.NodeType == NodeType.Process || x.NodeType == NodeType.Output);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="outputNodesOnly"></param>
        /// <param name="tryUsePreviousStepNodeOutput">Use the value in the last step when a connection is dependent on a circular reference.</param>
        /// <param name="defaultNodeOutputValue">The value to use when the output of a node is dependent on a connection with a circular reference or the previous step info is not present.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public Dictionary<int, float> Step(bool outputNodesOnly, float defaultNodeOutputValue = 0)
        {
            if (outputNodesOnly)
            {
                Dictionary<int, float> cachedNodeOutputs = new Dictionary<int, float>();
                List<int> inputNodeIds = new List<int>();
                List<int> outputNodeIds = new List<int>();

                foreach (var nodeIdToNodeKeyValuePair in NodeIdsToNodesDict)
                {
                    int nodeId = nodeIdToNodeKeyValuePair.Key;
                    Node node = nodeIdToNodeKeyValuePair.Value;

                    if (node.NodeType == NodeType.Input)
                    {
                        inputNodeIds.Add(nodeId);
                    }
                    else if (node.NodeType == NodeType.Output)
                    {
                        outputNodeIds.Add(nodeId);
                    }
                }

                List<int> nodesBeingEvaluated = new List<int>();
                List<int> nodeQueue = new List<int>();

                nodeQueue.AddRange(outputNodeIds);
                nodeQueue.AddRange(inputNodeIds);

                List<NeuralNetworkStepNodeInfo> currentStepNodeInfo = new List<NeuralNetworkStepNodeInfo>();

                while (nodeQueue.Count > 0)
                {
                    int currentNodeId = nodeQueue[nodeQueue.Count - 1];
                    Node currentNode = NodeIdsToNodesDict[currentNodeId];
                    nodesBeingEvaluated.Add(currentNodeId);
                    float? currentNodeWeightedSum;

                    if (currentNode.NodeType == NodeType.Input)
                    {
                        if (currentNode.CreatureInput == null)
                        {
                            throw new InvalidOperationException($"node type is {currentNode.NodeType} but {nameof(CreatureInput)} is null.");
                        }

                        currentNodeWeightedSum = InputValues[currentNode.CreatureInput.Value];
                    }
                    else
                    {
                        if (currentNode.NodeType == NodeType.Output && currentNode.CreatureAction == null)
                        {
                            throw new InvalidOperationException($"node type is {currentNode.NodeType} but {nameof(CreatureAction)} is null.");
                        }

                        currentNodeWeightedSum = 0;

                        List<Connection> connectionsToCurrentNode = Connections.Where(x => x.TargetId == currentNodeId).ToList();

                        foreach (var connection in connectionsToCurrentNode)
                        {
                            if (connection.TargetId == currentNodeId)
                            {
                                float sourceNodeOutput;
                                Node connectionSourceNode = NodeIdsToNodesDict[connection.SourceId];
                                bool isSelfReferencingConnection = connection.SourceId == connection.TargetId;

                                //If the source node's output needs to be retrieved and it is currently being evaluated,
                                //the only thing that can be done is use the cached value.
                                if (cachedNodeOutputs.TryGetValue(connection.SourceId, out float cachedSourceNodeOutput))
                                {
                                    sourceNodeOutput = cachedSourceNodeOutput;
                                }
                                else if (nodesBeingEvaluated.Contains(connection.SourceId))
                                {
                                    sourceNodeOutput = defaultNodeOutputValue;
                                }
                                else
                                {
                                    nodeQueue.Add(connection.SourceId);
                                    currentNodeWeightedSum = null;
                                    break;
                                }

                                currentNodeWeightedSum += sourceNodeOutput * connection.Weight;
                            }
                        }
                    }

                    if (currentNodeWeightedSum != null)
                    {
                        float currentNodeOutput = currentNode.NodeType == NodeType.Input ? currentNodeWeightedSum.Value : Globals.ReLU(currentNodeWeightedSum.Value + currentNode.Bias);

                        nodesBeingEvaluated.Remove(currentNodeId);

                        cachedNodeOutputs[currentNodeId] = currentNodeOutput;

                        nodeQueue.Remove(currentNodeId);

                        NeuralNetworkStepNodeInfo currentStepCurrentNodeInfo = currentStepNodeInfo.FirstOrDefault(x => x.NodeId == currentNodeId);

                        if (currentStepCurrentNodeInfo == null)
                        {
                            currentStepCurrentNodeInfo = new NeuralNetworkStepNodeInfo()
                            {
                                NodeId = currentNodeId,
                                Bias = currentNode.Bias,
                                CreatureAction = currentNode.CreatureAction,
                                CreatureInput = currentNode.CreatureInput,
                                NodeType = currentNode.NodeType
                            };

                            currentStepNodeInfo.Add(currentStepCurrentNodeInfo);
                        }

                        currentStepCurrentNodeInfo.PreviousOutput = currentNodeOutput;
                    }
                }

                foreach (var nodeInfo in currentStepNodeInfo)
                {
                    foreach (var connection in Connections)
                    {
                        bool sourceIdIsNodeId = connection.SourceId == nodeInfo.NodeId;
                        bool targetIdIsNodeId = connection.TargetId == nodeInfo.NodeId;

                        if (sourceIdIsNodeId || targetIdIsNodeId)
                        {
                            nodeInfo.Connections.Add(connection);
                        }

                        if (sourceIdIsNodeId)
                        {
                            nodeInfo.ConnectionsFrom.Add(connection);
                        }

                        if (targetIdIsNodeId)
                        {
                            nodeInfo.ConnectionsTo.Add(connection);
                        }
                    }
                }

                PreviousStepInfo.Clear();
                PreviousStepInfo.AddRange(currentStepNodeInfo);

                return cachedNodeOutputs;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public int AddNode(Node node)
        {
            bool nodeIdCreated = false;
            int nodeId = -1;

            for (int i = 0; !nodeIdCreated; i++)
            {
                if (!NodeIdsToNodesDict.ContainsKey(i))
                {
                    nodeId = i;
                    nodeIdCreated = true;
                }
            }

            _nodeIdsToNodesDict[nodeId] = node;
            _nodesToNodeIdsDict[node] = nodeId;

            return nodeId;
        }
        public bool RemoveNode(int nodeId, bool removeConnections)
        {
            bool nodeWasRemoved;

            if (NodeIdsToNodesDict.TryGetValue(nodeId, out Node node))
            {
                if (removeConnections)
                {
                    RemoveConnectionsToNode(nodeId);
                }

                _nodeIdsToNodesDict.Remove(nodeId);
                _nodesToNodeIdsDict.Remove(node);

                nodeWasRemoved = true;
            }
            else
            {
                nodeWasRemoved = false;
            }

            return nodeWasRemoved;
        }
        public void RemoveConnectionsToNode(int nodeId)
        {
            //Create a new list to prevent an exception caused by trying to modify a collection that is being iterated over.
            List<Connection> connectionsList = Connections.ToList();

            foreach (var connection in connectionsList)
            {
                if (connection.SourceId == nodeId || connection.TargetId == nodeId)
                {
                    Connections.Remove(connection);
                }
            }
        }
        public void RemoveConnectionsToNodes(IEnumerable<int> nodeIds)
        {
            //Create a new list to prevent an exception caused by trying to modify a collection that is being iterated over.
            List<Connection> connectionsList = Connections.ToList();

            foreach (var connection in connectionsList)
            {
                if (nodeIds.Any(x => x == connection.SourceId || x == connection.TargetId))
                {
                    Connections.Remove(connection);
                }
            }
        }
        #endregion
    }
}