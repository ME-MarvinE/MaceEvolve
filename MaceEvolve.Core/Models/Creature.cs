using MaceEvolve.Core.Enums;
using MaceEvolve.Core.Extensions;
using MaceEvolve.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MaceEvolve.Core.Models
{
    public class Creature : GameObject, ICreature
    {
        #region Fields
        private float _energy;
        #endregion

        #region Properties
        public NeuralNetwork Brain { get; set; }
        public float MoveCost { get; set; } = 0.5f;
        public float Energy
        {
            get
            {
                return _energy;
            }
            set
            {
                if (value < 0)
                {
                    _energy = 0;
                }
                else if (value > MaxEnergy)
                {
                    _energy = MaxEnergy;
                }
                else
                {
                    _energy = value;
                }
            }
        }
        public float MaxEnergy { get; set; } = 150;
        public float Speed { get; set; } = 1;
        public int SightRange { get; set; } = 200;
        public float Metabolism { get; set; } = 0.1f;
        public int FoodEaten { get; set; }
        public bool IsDead { get; set; }
        //public int StomachSize { get; set; } = 5;
        //public List<food> StomachContents { get; set; } = 5;
        //public float DigestionRate = 0.1;
        #endregion

        #region Methods
        public bool IsWithinSight(IGameObject gameObject)
        {
            return Globals.GetDistanceFrom(X, Y, gameObject.X, gameObject.Y) <= SightRange;
        }
        public void Die()
        {
            IsDead = true;
            Energy = 0;
        }
        public static T Reproduce<T>(IList<T> parents, List<CreatureInput> inputs, List<CreatureAction> actions, float nodeBiasMaxVariancePercentage, float connectionWeightMaxVariancePercentage, float connectionWeightBound) where T : ICreature, new()
        {
            Dictionary<T, List<Connection>> availableParentConnections = parents.ToDictionary(x => x, x => x.Brain.Connections.ToList());
            Dictionary<T, Dictionary<int, int>> parentToOffspringNodesMap = new Dictionary<T, Dictionary<int, int>>();

            T offspring = new T()
            {
                Brain = new NeuralNetwork(new List<Node>(), inputs, actions, new List<Connection>())
            };

            float averageNumberOfParentConnections = (float)parents.Average(x => x.Brain.Connections.Count);

            if (averageNumberOfParentConnections > 0 && averageNumberOfParentConnections < 1)
            {
                averageNumberOfParentConnections = 1;
            }

            while (offspring.Brain.Connections.Count < averageNumberOfParentConnections)
            {
                T randomParent = parents[Globals.Random.Next(parents.Count)];
                List<Connection> randomParentAvailableConnections = availableParentConnections[randomParent];

                if (randomParentAvailableConnections.Count > 0)
                {
                    Connection randomParentConnection = randomParentAvailableConnections[Globals.Random.Next(randomParentAvailableConnections.Count)];

                    //If a parent's node has not been added and mapped to an offspring's node, create a new node and map it to the parent's node.
                    if (!(parentToOffspringNodesMap.ContainsKey(randomParent) && parentToOffspringNodesMap[randomParent].ContainsKey(randomParentConnection.SourceId)))
                    {
                        Node randomParentConnectionSourceNode = randomParent.Brain.NodeIdsToNodesDict[randomParentConnection.SourceId];
                        Node newNode = new Node(randomParentConnectionSourceNode.NodeType, randomParentConnectionSourceNode.Bias, randomParentConnectionSourceNode.CreatureInput, randomParentConnectionSourceNode.CreatureAction);
                        int newNodeId = offspring.Brain.AddNode(newNode);

                        //Apply any variance to the node's bias.
                        newNode.Bias = Globals.Random.NextFloatVariance(randomParentConnectionSourceNode.Bias, nodeBiasMaxVariancePercentage);

                        if (newNode.Bias < -1)
                        {
                            newNode.Bias = -1;
                        }
                        else if (newNode.Bias > 1)
                        {
                            newNode.Bias = 1;
                        }

                        //Map the newly added offspring node to the parent's node so that duplicates aren't created if two of the parent's connections reference the same node.
                        if (!parentToOffspringNodesMap.ContainsKey(randomParent))
                        {
                            parentToOffspringNodesMap.Add(randomParent, new Dictionary<int, int>());
                        }

                        parentToOffspringNodesMap[randomParent][randomParentConnection.SourceId] = newNodeId;
                    }

                    if (!(parentToOffspringNodesMap.ContainsKey(randomParent) && parentToOffspringNodesMap[randomParent].ContainsKey(randomParentConnection.TargetId)))
                    {
                        Node randomParentConnectionTargetNode = randomParent.Brain.NodeIdsToNodesDict[randomParentConnection.TargetId];
                        Node newNode = new Node(randomParentConnectionTargetNode.NodeType, randomParentConnectionTargetNode.Bias, randomParentConnectionTargetNode.CreatureInput, randomParentConnectionTargetNode.CreatureAction);
                        int newNodeId = offspring.Brain.AddNode(newNode);

                        //Map the newly added offspring node to the parent's node so that duplicates aren't created if two of the parent's connections reference the same node.
                        if (!parentToOffspringNodesMap.ContainsKey(randomParent))
                        {
                            parentToOffspringNodesMap.Add(randomParent, new Dictionary<int, int>());
                        }

                        parentToOffspringNodesMap[randomParent][randomParentConnection.TargetId] = newNodeId;
                    }

                    Connection connectionToAdd = new Connection()
                    {
                        SourceId = parentToOffspringNodesMap[randomParent][randomParentConnection.SourceId],
                        TargetId = parentToOffspringNodesMap[randomParent][randomParentConnection.TargetId]
                    };

                    //Apply any variance to the connection's weight.
                    connectionToAdd.Weight = Globals.Random.NextFloatVariance(randomParentConnection.Weight, connectionWeightMaxVariancePercentage);

                    if (connectionToAdd.Weight < -connectionWeightBound)
                    {
                        connectionToAdd.Weight = -connectionWeightBound;
                    }
                    else if (connectionToAdd.Weight > connectionWeightBound)
                    {
                        connectionToAdd.Weight = connectionWeightBound;
                    }

                    offspring.Brain.Connections.Add(connectionToAdd);
                    availableParentConnections[randomParent].Remove(randomParentConnection);
                }
            }

            return offspring;
        }
        T ICreature.Reproduce<T>(IList<T> parents, List<CreatureInput> inputs, List<CreatureAction> actions, float nodeBiasMaxVariancePercentage, float connectionWeightMaxVariancePercentage, float connectionWeightBound)
        {
            return Reproduce(parents, inputs, actions, nodeBiasMaxVariancePercentage, connectionWeightMaxVariancePercentage, connectionWeightBound);
        }

        #endregion
    }
}
