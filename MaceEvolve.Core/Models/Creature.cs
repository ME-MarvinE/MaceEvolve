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
        private double _energy;
        #endregion

        #region Properties
        public NeuralNetwork Brain { get; set; }
        public double MoveCost { get; set; } = 0.5;
        public double Energy
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
        public double MaxEnergy { get; set; } = 150;
        public double Speed { get; set; } = 1;
        public int SightRange { get; set; } = 200;
        public double Metabolism { get; set; } = 0.1;
        public int FoodEaten { get; set; }
        public bool IsDead { get; set; }
        //public int StomachSize { get; set; } = 5;
        //public List<food> StomachContents { get; set; } = 5;
        //public double DigestionRate = 0.1;
        #endregion

        #region Methods
        public void ExecuteAction(CreatureAction creatureAction, CreatureStepInfo stepInfo)
        {
            switch (creatureAction)
            {
                case CreatureAction.MoveForward:
                    MoveForward(stepInfo);
                    break;

                case CreatureAction.MoveBackward:
                    MoveBackward(stepInfo);
                    break;

                case CreatureAction.MoveLeft:
                    MoveLeft(stepInfo);
                    break;

                case CreatureAction.MoveRight:
                    MoveRight(stepInfo);
                    break;

                case CreatureAction.TryEat:
                    TryEatFoodInRange(stepInfo);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
        public IEnumerable<T> GetVisibleGameObjects<T>(IEnumerable<T> gameObjects) where T : IGameObject
        {
            return gameObjects.Where(x => Globals.GetDistanceFrom(X, Y, x.X, x.Y) <= SightRange);
        }
        public void Die()
        {
            IsDead = true;
            Energy = 0;
        }
        public void Live(EnvironmentInfo currentEnvironmentInfo)
        {
            CreatureStepInfo currentStepInfo = CreateStepInfo(currentEnvironmentInfo);

            UpdateInputValues(currentStepInfo);

            Dictionary<int, double> nodeIdToOutputDict = Brain.Step(true);
            Dictionary<Node, double> nodeOutputsDict = nodeIdToOutputDict.OrderBy(x => x.Value).ToDictionary(x => Brain.NodeIdsToNodesDict[x.Key], x => x.Value);
            Node highestOutputNode = nodeOutputsDict.Keys.LastOrDefault(x => x.NodeType == NodeType.Output);

            if (highestOutputNode != null && nodeOutputsDict[highestOutputNode] > 0)
            {
                ExecuteAction(highestOutputNode.CreatureAction.Value, currentStepInfo);
            }

            Energy -= Metabolism;
        }
        public CreatureStepInfo CreateStepInfo(EnvironmentInfo environmentInfo)
        {
            CreatureStepInfo stepInfo = new CreatureStepInfo();

            stepInfo.EnvironmentInfo = environmentInfo;
            stepInfo.VisibleFood = GetVisibleGameObjects(stepInfo.EnvironmentInfo.ExistingFood).ToList();
            stepInfo.VisibleCreatures = GetVisibleGameObjects(stepInfo.EnvironmentInfo.ExistingCreatures).Where(x => x != this).ToList();
            stepInfo.VisibleFoodOrderedByDistance = stepInfo.VisibleFood.OrderBy(x => Globals.GetDistanceFrom(X, Y, x.X, x.Y)).ToList();
            stepInfo.VisibleCreaturesOrderedByDistance = stepInfo.VisibleCreatures.OrderBy(x => Globals.GetDistanceFrom(X, Y, x.X, x.Y)).ToList();

            return stepInfo;
        }
        public void UpdateInputValues(CreatureStepInfo stepInfo)
        {
            Brain.UpdateInputValue(CreatureInput.PercentMaxEnergy, PercentMaxEnergy());
            Brain.UpdateInputValue(CreatureInput.ProximityToCreatureToLeft, ProximityToCreatureToLeft(stepInfo));
            Brain.UpdateInputValue(CreatureInput.ProximityToCreatureToRight, ProximityToCreatureToRight(stepInfo));
            Brain.UpdateInputValue(CreatureInput.ProximityToCreatureToFront, ProximityToCreatureToFront(stepInfo));
            Brain.UpdateInputValue(CreatureInput.ProximityToCreatureToBack, ProximityToCreatureToBack(stepInfo));
            Brain.UpdateInputValue(CreatureInput.ProximityToFoodToLeft, ProximityToFoodToLeft(stepInfo));
            Brain.UpdateInputValue(CreatureInput.ProximityToFoodToRight, ProximityToFoodToRight(stepInfo));
            Brain.UpdateInputValue(CreatureInput.ProximityToFoodToFront, ProximityToFoodToFront(stepInfo));
            Brain.UpdateInputValue(CreatureInput.ProximityToFoodToBack, ProximityToFoodToBack(stepInfo));
            Brain.UpdateInputValue(CreatureInput.DistanceFromTopWorldBound, DistanceFromTopWorldBound(stepInfo));
            Brain.UpdateInputValue(CreatureInput.DistanceFromLeftWorldBound, DistanceFromLeftWorldBound(stepInfo));
            Brain.UpdateInputValue(CreatureInput.RandomInput, RandomInput());
        }
        public static T Reproduce<T>(IList<T> parents, List<CreatureInput> inputs, List<CreatureAction> actions, double nodeBiasMaxVariancePercentage, double connectionWeightMaxVariancePercentage, double connectionWeightBound) where T : ICreature, new()
        {
            Dictionary<T, List<Connection>> availableParentConnections = parents.ToDictionary(x => x, x => x.Brain.Connections.ToList());
            Dictionary<T, Dictionary<int, int>> parentToOffspringNodesMap = new Dictionary<T, Dictionary<int, int>>();

            T offspring = new T()
            {
                Brain = new NeuralNetwork(new List<Node>(), inputs, actions, new List<Connection>())
            };

            double averageNumberOfParentConnections = parents.Average(x => x.Brain.Connections.Count);

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
                        newNode.Bias = Globals.Random.NextDoubleVariance(randomParentConnectionSourceNode.Bias, nodeBiasMaxVariancePercentage);

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
                    connectionToAdd.Weight = Globals.Random.NextDoubleVariance(randomParentConnection.Weight, connectionWeightMaxVariancePercentage);

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
        T ICreature.Reproduce<T>(IList<T> parents, List<CreatureInput> inputs, List<CreatureAction> actions, double nodeBiasMaxVariancePercentage, double connectionWeightMaxVariancePercentage, double connectionWeightBound)
        {
            return Reproduce(parents, inputs, actions, nodeBiasMaxVariancePercentage, connectionWeightMaxVariancePercentage, connectionWeightBound);
        }

        #region CreatureValues
        //x values map from 0 to 1.
        public double PercentMaxEnergy()
        {
            return Globals.Map(Energy, 0, MaxEnergy, 0, 1);
        }
        public double ProximityToCreatureToLeft(CreatureStepInfo stepInfo)
        {
            if (stepInfo.VisibleCreaturesOrderedByDistance.Count == 0)
            {
                return 0;
            }

            ICreature closestCreature = stepInfo.VisibleCreaturesOrderedByDistance.FirstOrDefault(x => x.MX < MX);

            if (closestCreature == null)
            {
                return 0;
            }

            double distanceFromClosestCreatureToLeft = Globals.GetDistanceFrom(MX, MY, closestCreature.MX, MY);

            if (distanceFromClosestCreatureToLeft < 0)
            {
                return 0;
            }

            return Globals.Map(distanceFromClosestCreatureToLeft, 0, SightRange, 1, 0);
        }
        public double ProximityToCreatureToRight(CreatureStepInfo stepInfo)
        {
            if (stepInfo.VisibleCreaturesOrderedByDistance.Count == 0)
            {
                return 0;
            }

            ICreature closestCreature = stepInfo.VisibleCreaturesOrderedByDistance.FirstOrDefault(x => x.MX > MX);

            if (closestCreature == null)
            {
                return 0;
            }

            double distanceFromClosestCreatureToRight = Globals.GetDistanceFrom(MX, MY, closestCreature.MX, MY);

            if (distanceFromClosestCreatureToRight < 0)
            {
                return 0;
            }

            return Globals.Map(distanceFromClosestCreatureToRight, 0, SightRange, 1, 0);
        }
        public double ProximityToCreatureToFront(CreatureStepInfo stepInfo)
        {
            if (stepInfo.VisibleCreaturesOrderedByDistance.Count == 0)
            {
                return 0;
            }

            ICreature closestCreature = stepInfo.VisibleCreaturesOrderedByDistance.FirstOrDefault(x => x.MY < MY);

            if (closestCreature == null)
            {
                return 0;
            }

            double distanceFromClosestCreatureToFront = Globals.GetDistanceFrom(MX, MY, MX, closestCreature.MY);

            if (distanceFromClosestCreatureToFront < 0)
            {
                return 0;
            }

            return Globals.Map(distanceFromClosestCreatureToFront, 0, SightRange, 1, 0);
        }
        public double ProximityToCreatureToBack(CreatureStepInfo stepInfo)
        {
            if (stepInfo.VisibleCreaturesOrderedByDistance.Count == 0)
            {
                return 0;
            }

            ICreature closestCreature = stepInfo.VisibleCreaturesOrderedByDistance.FirstOrDefault(x => x.MY > MY);

            if (closestCreature == null)
            {
                return 0;
            }

            double distanceFromClosestCreatureToBack = Globals.GetDistanceFrom(MX, MY, MX, closestCreature.MY);

            if (distanceFromClosestCreatureToBack < 0)
            {
                return 0;
            }

            return Globals.Map(distanceFromClosestCreatureToBack, 0, SightRange, 1, 0);
        }
        public double ProximityToFoodToLeft(CreatureStepInfo stepInfo)
        {
            if (stepInfo.VisibleFoodOrderedByDistance.Count == 0)
            {
                return 0;
            }

            IFood closestFood = stepInfo.VisibleFoodOrderedByDistance.FirstOrDefault(x => x.MX < MX);

            if (closestFood == null)
            {
                return 0;
            }

            double distanceFromClosestFoodToLeft = Globals.GetDistanceFrom(MX, MY, closestFood.MX, MY);

            if (distanceFromClosestFoodToLeft < 0)
            {
                return 0;
            }

            return Globals.Map(distanceFromClosestFoodToLeft, 0, SightRange, 1, 0);
        }
        public double ProximityToFoodToRight(CreatureStepInfo stepInfo)
        {
            if (stepInfo.VisibleFoodOrderedByDistance.Count == 0)
            {
                return 0;
            }

            IFood closestFood = stepInfo.VisibleFoodOrderedByDistance.FirstOrDefault(x => x.MX > MX);

            if (closestFood == null)
            {
                return 0;
            }

            double distanceFromClosestFoodToRight = Globals.GetDistanceFrom(MX, MY, closestFood.MX, MY);

            if (distanceFromClosestFoodToRight < 0)
            {
                return 0;
            }

            return Globals.Map(distanceFromClosestFoodToRight, 0, SightRange, 1, 0);
        }
        public double ProximityToFoodToFront(CreatureStepInfo stepInfo)
        {
            if (stepInfo.VisibleFoodOrderedByDistance.Count == 0)
            {
                return 0;
            }

            IFood closestFood = stepInfo.VisibleFoodOrderedByDistance.FirstOrDefault(x => x.MY < MY);

            if (closestFood == null)
            {
                return 0;
            }

            double distanceFromClosestFoodToFront = Globals.GetDistanceFrom(MX, MY, MX, closestFood.MY);

            if (distanceFromClosestFoodToFront < 0)
            {
                return 0;
            }

            return Globals.Map(distanceFromClosestFoodToFront, 0, SightRange, 1, 0);
        }
        public double ProximityToFoodToBack(CreatureStepInfo stepInfo)
        {
            if (stepInfo.VisibleFoodOrderedByDistance.Count == 0)
            {
                return 0;
            }

            IFood closestFood = stepInfo.VisibleFoodOrderedByDistance.FirstOrDefault(x => x.MY > MY);

            if (closestFood == null)
            {
                return 0;
            }

            double distanceFromClosestFoodToBack = Globals.GetDistanceFrom(MX, MY, MX, closestFood.MY);

            if (distanceFromClosestFoodToBack < 0)
            {
                return 0;
            }

            return Globals.Map(distanceFromClosestFoodToBack, 0, SightRange, 1, 0);
        }
        public double DistanceFromTopWorldBound(CreatureStepInfo stepInfo)
        {
            return Globals.Map(Y, stepInfo.EnvironmentInfo.WorldBounds.Y, stepInfo.EnvironmentInfo.WorldBounds.Y + stepInfo.EnvironmentInfo.WorldBounds.Height, 0, 1);
        }
        public double DistanceFromLeftWorldBound(CreatureStepInfo stepInfo)
        {
            return Globals.Map(X, stepInfo.EnvironmentInfo.WorldBounds.X, stepInfo.EnvironmentInfo.WorldBounds.X + stepInfo.EnvironmentInfo.WorldBounds.Width, 0, 1);
        }
        public double RandomInput()
        {
            return Globals.Random.NextDouble();
        }
        #endregion

        #region Actions
        private void Eat(IFood food)
        {
            Energy -= food.ServingDigestionCost;
            food.Servings -= 1;
            FoodEaten += 1;
            Energy += food.EnergyPerServing;
        }
        public bool TryEatFoodInRange(CreatureStepInfo stepInfo)
        {
            if (stepInfo.VisibleFoodOrderedByDistance.Count == 0)
            {
                return false;
            }

            IFood closestFood = stepInfo.VisibleFoodOrderedByDistance[0];

            if (Globals.GetDistanceFrom(MX, MY, closestFood.MX, closestFood.MY) > Size / 2)
            {
                return false;
            }

            Eat(closestFood);

            return true;
        }
        public void MoveForward(CreatureStepInfo stepInfo)
        {
            Y -= Speed;
            if (MY < stepInfo.EnvironmentInfo.WorldBounds.Y)
            {
                Y += Speed;
                //Y += WorldBounds.WorldBounds.Height;
            }
            Energy -= MoveCost;
        }
        public void MoveBackward(CreatureStepInfo stepInfo)
        {
            Y += Speed;
            double worldBoundsBottom = stepInfo.EnvironmentInfo.WorldBounds.Y + stepInfo.EnvironmentInfo.WorldBounds.Height;
            if (MY > worldBoundsBottom)
            {
                Y -= Speed;
                //Y -= WorldBounds.WorldBounds.Height;
            }
            Energy -= MoveCost;
        }
        public void MoveLeft(CreatureStepInfo stepInfo)
        {
            X -= Speed;
            if (MX < stepInfo.EnvironmentInfo.WorldBounds.X)
            {
                X += Speed;
                //X += WorldBounds.WorldBounds.Width;
            }
            Energy -= MoveCost;
        }
        public void MoveRight(CreatureStepInfo stepInfo)
        {
            X += Speed;
            double worldBoundsRight = stepInfo.EnvironmentInfo.WorldBounds.X + stepInfo.EnvironmentInfo.WorldBounds.Width;
            if (MX > worldBoundsRight)
            {
                X -= Speed;
                //X -= WorldBounds.WorldBounds.Width;
            }
            Energy -= MoveCost;
        }
        public void MoveTowardsClosestFood(CreatureStepInfo stepInfo)
        {
            if (stepInfo.VisibleFoodOrderedByDistance.Count > 0)
            {
                IFood closestFood = stepInfo.VisibleFoodOrderedByDistance[0];

                double xDifference = X - closestFood.X;
                double yDifference = Y - closestFood.Y;

                if (xDifference + yDifference <= SightRange)
                {
                    if (yDifference > 0)
                    {
                        if (yDifference >= Speed)
                        {
                            MoveForward(stepInfo);
                        }
                    }
                    else if (yDifference < 0)
                    {
                        if (yDifference <= -Speed)
                        {
                            MoveBackward(stepInfo);
                        }
                    }

                    if (xDifference > 0)
                    {
                        if (xDifference >= Speed)
                        {
                            MoveLeft(stepInfo);
                        }
                    }
                    else if (xDifference < 0)
                    {
                        if (xDifference <= -Speed)
                        {
                            MoveRight(stepInfo);
                        }
                    }
                }
            }
        }
        #endregion

        #endregion
    }
}
