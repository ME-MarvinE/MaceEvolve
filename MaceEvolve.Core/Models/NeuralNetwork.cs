using MaceEvolve.Core.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace MaceEvolve.Core.Models
{
    public class NeuralNetwork
    {
        #region Fields
        private readonly Dictionary<int, Node> _nodeIdsToNodesDict = new Dictionary<int, Node>();
        #endregion

        #region Properties
        public List<Connection> Connections { get; } = new List<Connection>();

        public IReadOnlyDictionary<int, Node> NodeIdsToNodesDict { get; }
        #endregion

        #region Constructors
        public NeuralNetwork()
            : this(Enumerable.Empty<Node>())
        {
        }
        public NeuralNetwork(IEnumerable<Node> nodes)
        {
            if (nodes == null) { throw new ArgumentNullException(nameof(nodes)); }

            NodeIdsToNodesDict = new ReadOnlyDictionary<int, Node>(_nodeIdsToNodesDict);

            foreach (var node in nodes)
            {
                AddNode(node);
            }
        }
        #endregion

        #region Methods
        public List<Connection> GenerateRandomConnections(int minConnections, int maxConnections, float weightBound)
        {
            List<Connection> generatedConnections = new List<Connection>();
            int targetConnectionAmount = MaceRandom.Current.Next(minConnections, maxConnections + 1);

            List<int> possibleSourceNodesIds = GetNodeIds((_, node) => node.NodeType == NodeType.Input || node.NodeType == NodeType.Process);
            List<int> possibleTargetNodesIds = GetNodeIds((_, node) => node.NodeType == NodeType.Output || node.NodeType == NodeType.Process);

            if (possibleSourceNodesIds.Count == 0) { throw new InvalidOperationException("No possible source nodes."); }
            if (possibleTargetNodesIds.Count == 0) { throw new InvalidOperationException("No possible target nodes."); }

            while (generatedConnections.Count < targetConnectionAmount)
            {
                int randomConnectionSource = possibleSourceNodesIds[MaceRandom.Current.Next(0, possibleSourceNodesIds.Count)];
                int randomConnectionTarget = possibleTargetNodesIds[MaceRandom.Current.Next(0, possibleTargetNodesIds.Count)];

                Connection newConnection = new Connection(randomConnectionSource, randomConnectionTarget, MaceRandom.Current.NextFloat(-weightBound, weightBound));
                generatedConnections.Add(newConnection);
            }

            return generatedConnections;
        }
        public static List<Node> GenerateInputNodes(IEnumerable<CreatureInput> possibleInputs)
        {
            return possibleInputs.Select(x => new Node(NodeType.Input, MaceRandom.Current.NextFloat(-1, 1), creatureInput: x)).ToList();
        }
        public static List<Node> GenerateOutputNodes(IEnumerable<CreatureAction> possibleOutputs)
        {
            return possibleOutputs.Select(x => new Node(NodeType.Output, MaceRandom.Current.NextFloat(-1, 1), creatureAction: x)).ToList();
        }
        public static List<Node> GenerateProcessNodes(int maxProcessNodes, float processNodeCreationChance)
        {
            List<Node> processNodes = new List<Node>();

            for (int i = 0; i < maxProcessNodes; i++)
            {
                if (MaceRandom.Current.NextFloat() < processNodeCreationChance)
                {
                    processNodes.Add(new Node(NodeType.Process, MaceRandom.Current.NextFloat(-1, 1)));
                }
            }

            return processNodes;
        }
        public NeuralNetwork CloneNetwork()
        {
            return JsonConvert.DeserializeObject<NeuralNetwork>(JsonConvert.SerializeObject(this));
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
        public List<int> GetNodeIds(Expression<Func<int, Node, bool>> predicate = null)
        {
            if (predicate == null)
            {
                return NodeIdsToNodesDict.Keys.ToList();
            }

            Func<int, Node, bool> predicateFunc = predicate.Compile();

            List<int> nodeIds = new List<int>();

            foreach (var keyValuePair in NodeIdsToNodesDict)
            {
                int nodeId = keyValuePair.Key;
                Node node = keyValuePair.Value;

                if (predicateFunc.Invoke(nodeId, node))
                {
                    nodeIds.Add(nodeId);
                }
            }

            return nodeIds;
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
        public Dictionary<int, float> GenerateNodeOutputs(Dictionary<CreatureInput, float> inputsToInputValuesDict, float defaultNodeOutputValue = 0)
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

                    currentNodeWeightedSum = inputsToInputValuesDict[currentNode.CreatureInput.Value];
                }
                else
                {
                    if (currentNode.NodeType == NodeType.Output && currentNode.CreatureAction == null)
                    {
                        throw new InvalidOperationException($"node type is {currentNode.NodeType} but {nameof(CreatureAction)} is null.");
                    }

                    currentNodeWeightedSum = 0;

                    IEnumerable<Connection> connectionsToCurrentNode = Connections.Where(x => x.TargetId == currentNodeId);

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
                }
            }

            return cachedNodeOutputs;
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

            _nodeIdsToNodesDict.Add(nodeId, node);

            return nodeId;
        }
        public bool RemoveNode(int nodeId, bool removeConnections)
        {
            if (NodeIdsToNodesDict.TryGetValue(nodeId, out Node node))
            {
                if (removeConnections)
                {
                    RemoveConnectionsToNode(nodeId);
                }

                _nodeIdsToNodesDict.Remove(nodeId);

                return true;
            }

            return false;
        }
        public bool ReplaceNode(int existingNodeId, Node newNode)
        {
            if (NodeIdsToNodesDict.TryGetValue(existingNodeId, out Node node))
            {
                _nodeIdsToNodesDict[existingNodeId] = newNode;

                return true;
            }

            return false;
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
        public IEnumerable<CreatureInput> GetInputsRequiredForStep()
        {
            List<CreatureInput> requiredInputs = new List<CreatureInput>();

            foreach (var keyValuePair in NodeIdsToNodesDict)
            {
                Node node = keyValuePair.Value;

                if (node.NodeType == NodeType.Input)
                {
                    if (node.CreatureInput == null)
                    {
                        int nodeId = keyValuePair.Key;

                        throw new InvalidOperationException($"Node with Id '{nodeId}' is an input but it's {nameof(node.CreatureInput)} is null.");
                    }

                    requiredInputs.Add(node.CreatureInput.Value);
                }
            }

            return requiredInputs;
        }
        public static NeuralNetwork CombineNetworks(IEnumerable<NeuralNetwork> networksToCombine)
        {
            Dictionary<NeuralNetwork, List<Connection>> availableParentConnections = networksToCombine.ToDictionary(x => x, x => x.Connections.ToList());
            Dictionary<NeuralNetwork, Dictionary<int, int>> parentToNewNetworkNodesMap = new Dictionary<NeuralNetwork, Dictionary<int, int>>();

            int networksToCombineCount = networksToCombine.Count();

            NeuralNetwork newNetwork = new NeuralNetwork();

            float averageNumberOfParentConnections = networksToCombine.Average(x => (float)x.Connections.Count);

            if (averageNumberOfParentConnections > 0 && averageNumberOfParentConnections < 1)
            {
                averageNumberOfParentConnections = 1;
            }

            while (newNetwork.Connections.Count < averageNumberOfParentConnections)
            {
                NeuralNetwork randomParent = networksToCombine.ElementAt(MaceRandom.Current.Next(networksToCombineCount));
                List<Connection> randomParentAvailableConnections = availableParentConnections[randomParent];

                if (randomParentAvailableConnections.Count > 0)
                {
                    Connection randomParentConnection = randomParentAvailableConnections[MaceRandom.Current.Next(randomParentAvailableConnections.Count)];

                    //If a parent's node has not been added and mapped to a newNetwork's node, create a new node and map it to the parent's node.
                    if (!(parentToNewNetworkNodesMap.ContainsKey(randomParent) && parentToNewNetworkNodesMap[randomParent].ContainsKey(randomParentConnection.SourceId)))
                    {
                        Node randomParentConnectionSourceNode = randomParent.NodeIdsToNodesDict[randomParentConnection.SourceId];
                        Node newNode = new Node(randomParentConnectionSourceNode.NodeType, randomParentConnectionSourceNode.Bias, randomParentConnectionSourceNode.CreatureInput, randomParentConnectionSourceNode.CreatureAction);
                        int newNodeId = newNetwork.AddNode(newNode);

                        //Map the newly added newNetwork node to the parent's node so that duplicates aren't created if two of the parent's connections reference the same node.
                        if (!parentToNewNetworkNodesMap.ContainsKey(randomParent))
                        {
                            parentToNewNetworkNodesMap.Add(randomParent, new Dictionary<int, int>());
                        }

                        parentToNewNetworkNodesMap[randomParent][randomParentConnection.SourceId] = newNodeId;
                    }

                    if (!(parentToNewNetworkNodesMap.ContainsKey(randomParent) && parentToNewNetworkNodesMap[randomParent].ContainsKey(randomParentConnection.TargetId)))
                    {
                        Node randomParentConnectionTargetNode = randomParent.NodeIdsToNodesDict[randomParentConnection.TargetId];
                        Node newNode = new Node(randomParentConnectionTargetNode.NodeType, randomParentConnectionTargetNode.Bias, randomParentConnectionTargetNode.CreatureInput, randomParentConnectionTargetNode.CreatureAction);
                        int newNodeId = newNetwork.AddNode(newNode);

                        //Map the newly added newNetwork node to the parent's node so that duplicates aren't created if two of the parent's connections reference the same node.
                        if (!parentToNewNetworkNodesMap.ContainsKey(randomParent))
                        {
                            parentToNewNetworkNodesMap.Add(randomParent, new Dictionary<int, int>());
                        }

                        parentToNewNetworkNodesMap[randomParent][randomParentConnection.TargetId] = newNodeId;
                    }

                    //Apply any variance to the connection's weight.
                    float connectionToAddWeight = randomParentConnection.Weight;
                    int connectionToAddSourceId = parentToNewNetworkNodesMap[randomParent][randomParentConnection.SourceId];
                    int connectionToAddTargetId = parentToNewNetworkNodesMap[randomParent][randomParentConnection.TargetId];

                    newNetwork.Connections.Add(new Connection(connectionToAddSourceId, connectionToAddTargetId, connectionToAddWeight));
                    availableParentConnections[randomParent].Remove(randomParentConnection);
                }
            }

            return newNetwork;
        }
        public bool MutateNetwork(float createRandomNodeChance, float removeRandomNodeChance, float mutateRandomNodeBiasChance, float createRandomConnectionChance, float removeRandomConnectionChance, float mutateRandomConnectionSourceChance, float mutateRandomConnectionTargetChance, float mutateRandomConnectionWeightChance, IEnumerable<CreatureInput> possibleInputs, IEnumerable<CreatureAction> possibleOutputs, int minCreatureConnections, int maxCreatureConnections, int maxCreatureProcessNodes, float connectionWeightBound)
        {
            int processNodeCount = GetNodeIds((_, node) => node.NodeType == NodeType.Process).Count;
            bool mutationAttempted = false;

            //Things should be removed before being added so that there isn't a chance that the newly added thing is deleted straight after.
            //Connections should be added after nodes are added so that there is a chance the newly created node gets a connection.

            //Remove an existing node. Input nodes should not be removed. 
            if (MaceRandom.Current.NextFloat() <= removeRandomNodeChance)
            {
                mutationAttempted = true;

                List<int> processNodeIds = GetNodeIds(predicate: (_, node) => node.NodeType == NodeType.Process);
                List<int> outputNodeIds = GetNodeIds(predicate: (_, node) => node.NodeType == NodeType.Output);

                List<int> possibleNodeIdsToRemove = new List<int>(processNodeIds);

                //There must be at least one target node in a network.
                if (outputNodeIds.Count > 1)
                {
                    possibleNodeIdsToRemove.AddRange(outputNodeIds);
                }

                if (possibleNodeIdsToRemove.Count > 0)
                {
                    int nodeIdToRemove = possibleNodeIdsToRemove[MaceRandom.Current.Next(possibleNodeIdsToRemove.Count)];

                    RemoveNode(nodeIdToRemove, true);
                }
            }

            //Change a random node's bias.
            if (MaceRandom.Current.NextFloat() <= mutateRandomNodeBiasChance)
            {
                mutationAttempted = true;

                int randomNodeId = GetNodeIds()[MaceRandom.Current.Next(NodeIdsToNodesDict.Count)];
                Node randomNode = NodeIdsToNodesDict[randomNodeId];
                Node newNode = new Node(randomNode.NodeType, MaceRandom.Current.NextFloat(-1, 1), randomNode.CreatureInput, randomNode.CreatureAction);

                ReplaceNode(randomNodeId, newNode);
            }

            //Create a new node with a default connection.
            if (MaceRandom.Current.NextFloat() <= createRandomNodeChance)
            {
                List<CreatureInput> possibleCreatureInputsToAdd = GetPossibleInputsToAdd(possibleInputs).ToList();
                List<CreatureAction> possibleCreatureActionsToAdd = GetPossibleActionsToAdd(possibleOutputs).ToList();

                mutationAttempted = true;

                Node nodeToAdd;
                float nodeTypeRandomNum = MaceRandom.Current.NextFloat();
                float chanceForSingleNodeType = 1f / Globals.AllNodeTypes.Count;

                if (nodeTypeRandomNum <= chanceForSingleNodeType && possibleCreatureInputsToAdd.Count > 0)
                {
                    nodeToAdd = new Node(NodeType.Input, MaceRandom.Current.NextFloat(-1, 1), possibleCreatureInputsToAdd[MaceRandom.Current.Next(possibleCreatureInputsToAdd.Count)]);
                }
                else if (nodeTypeRandomNum <= chanceForSingleNodeType * 2 && possibleCreatureActionsToAdd.Count > 0)
                {
                    nodeToAdd = new Node(NodeType.Output, MaceRandom.Current.NextFloat(-1, 1), creatureAction: possibleCreatureActionsToAdd[MaceRandom.Current.Next(possibleCreatureActionsToAdd.Count)]);
                }
                else if (processNodeCount < maxCreatureProcessNodes)
                {
                    nodeToAdd = new Node(NodeType.Process, MaceRandom.Current.NextFloat(-1, 1));
                }
                else
                {
                    nodeToAdd = null;
                }

                if (nodeToAdd != null)
                {
                    List<int> possibleSourceNodesIds = GetNodeIds((_, node) => node.NodeType == NodeType.Input || node.NodeType == NodeType.Process);
                    List<int> possibleTargetNodesIds = GetNodeIds((_, node) => node.NodeType == NodeType.Output || node.NodeType == NodeType.Process);

                    int nodeToAddId = AddNode(nodeToAdd);

                    if (Connections.Count < maxCreatureConnections && possibleSourceNodesIds.Count > 0 && possibleTargetNodesIds.Count > 0)
                    {
                        Connection newConnection;
                        float newConnectionWeight = MaceRandom.Current.NextFloat(-connectionWeightBound, connectionWeightBound);

                        switch (nodeToAdd.NodeType)
                        {
                            case NodeType.Input:
                                newConnection = new Connection(nodeToAddId, possibleTargetNodesIds[MaceRandom.Current.Next(possibleTargetNodesIds.Count)], newConnectionWeight);
                                break;

                            case NodeType.Process:
                                if (MaceRandom.Current.NextDouble() <= 0.5)
                                {
                                    newConnection = new Connection(nodeToAddId, possibleTargetNodesIds[MaceRandom.Current.Next(possibleTargetNodesIds.Count)], newConnectionWeight);
                                }
                                else
                                {
                                    newConnection = new Connection(possibleSourceNodesIds[MaceRandom.Current.Next(possibleSourceNodesIds.Count)], nodeToAddId, newConnectionWeight);
                                }
                                break;

                            case NodeType.Output:
                                newConnection = new Connection(possibleSourceNodesIds[MaceRandom.Current.Next(possibleSourceNodesIds.Count)], nodeToAddId, newConnectionWeight);
                                break;

                            default:
                                throw new NotImplementedException();
                        }

                        Connections.Add(newConnection);
                    }
                }
            }

            //Remove a random connection.
            if (Connections.Count > minCreatureConnections && MaceRandom.Current.NextFloat() <= removeRandomConnectionChance)
            {
                mutationAttempted = true;

                Connection randomConnection = Connections[MaceRandom.Current.Next(Connections.Count)];
                Connections.Remove(randomConnection);
            }

            //Change a random connection's weight.
            if (Connections.Count > 0 && MaceRandom.Current.NextFloat() <= mutateRandomConnectionWeightChance)
            {
                mutationAttempted = true;

                int randomConnectionIndex = MaceRandom.Current.Next(Connections.Count);

                Connections[randomConnectionIndex] = new Connection(Connections[randomConnectionIndex].SourceId, Connections[randomConnectionIndex].TargetId, MaceRandom.Current.NextFloat(-connectionWeightBound, connectionWeightBound));
            }

            //Change a random connection's source.
            if (Connections.Count > 0 && MaceRandom.Current.NextFloat() <= mutateRandomConnectionSourceChance)
            {
                mutationAttempted = true;

                int randomConnectionIndex = MaceRandom.Current.Next(Connections.Count);
                List<int> possibleSourceNodesIds = GetNodeIds(predicate: (_, node) => node.NodeType == NodeType.Input || node.NodeType == NodeType.Process);

                if (possibleSourceNodesIds.Count > 0)
                {
                    int randomSourceNodeId = possibleSourceNodesIds[MaceRandom.Current.Next(possibleSourceNodesIds.Count)];

                    Connection mutatedConnection = new Connection(randomSourceNodeId, Connections[randomConnectionIndex].TargetId, Connections[randomConnectionIndex].Weight);
                    Connections[randomConnectionIndex] = mutatedConnection;
                }
            }

            //Change a random connection's target.
            if (Connections.Count > 0 && MaceRandom.Current.NextFloat() <= mutateRandomConnectionTargetChance)
            {
                mutationAttempted = true;

                int randomConnectionIndex = MaceRandom.Current.Next(Connections.Count);
                List<int> possibleTargetNodesIds = GetNodeIds(predicate: (_, node) => node.NodeType == NodeType.Output || node.NodeType == NodeType.Process);

                if (possibleTargetNodesIds.Count > 0)
                {
                    int randomTargetNodeId = possibleTargetNodesIds[MaceRandom.Current.Next(possibleTargetNodesIds.Count)];

                    Connection mutatedConnection = new Connection(Connections[randomConnectionIndex].SourceId, randomTargetNodeId, Connections[randomConnectionIndex].Weight);
                    Connections[randomConnectionIndex] = mutatedConnection;
                }
            }

            //Create a new connection.
            if (Connections.Count < maxCreatureConnections && MaceRandom.Current.NextDouble() <= createRandomConnectionChance)
            {
                mutationAttempted = true;

                Connection? newConnection = GenerateRandomConnections(1, 1, connectionWeightBound).FirstOrDefault();

                if (newConnection != null)
                {
                    Connections.Add(newConnection.Value);
                }
            }

            return mutationAttempted;
        }
        protected virtual IEnumerable<CreatureInput> GetPossibleInputsToAdd(IEnumerable<CreatureInput> allowedInputs)
        {
            //Return any inputs that aren't already used by a node in the network.
            return allowedInputs.Where(x => !NodeIdsToNodesDict.Any(y => y.Value.NodeType == NodeType.Input && x == y.Value.CreatureInput));
        }
        protected virtual IEnumerable<CreatureAction> GetPossibleActionsToAdd(IEnumerable<CreatureAction> allowedOutputs)
        {
            //Return any actions that aren't already used by a node in the network.
            return allowedOutputs.Where(x => !NodeIdsToNodesDict.Any(y => y.Value.NodeType == NodeType.Output && x == y.Value.CreatureAction));
        }
        #endregion
    }
}