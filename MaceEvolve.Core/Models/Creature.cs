using MaceEvolve.Core.Enums;
using MaceEvolve.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MaceEvolve.Core.Models
{
    public class Creature : GameObject
    {
        #region Fields
        private double _energy;
        #endregion

        #region Properties
        public NeuralNetwork Brain { get; set; }
        private double MoveCost { get; set; } = 0.5;
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
        public IEnumerable<T> GetVisibleGameObjects<T>(IEnumerable<T> gameObjects) where T : GameObject
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
            Brain.UpdateInputValue(CreatureInput.VerticalProximityToFood, VerticalProximityToFood(stepInfo));
            Brain.UpdateInputValue(CreatureInput.HorizontalProximityToFood, HorizontalProximityToFood(stepInfo));
            Brain.UpdateInputValue(CreatureInput.VerticalProximityToCreature, VerticalProximityToCreature(stepInfo));
            Brain.UpdateInputValue(CreatureInput.HorizontalProximityToCreature, HorizontalProximityToCreature(stepInfo));
            Brain.UpdateInputValue(CreatureInput.DistanceFromTopWorldBound, DistanceFromTopWorldBound(stepInfo));
            Brain.UpdateInputValue(CreatureInput.DistanceFromLeftWorldBound, DistanceFromLeftWorldBound(stepInfo));
            Brain.UpdateInputValue(CreatureInput.RandomInput, RandomInput());
        }
        public static Creature Reproduce(IList<Creature> parents, List<CreatureInput> inputs, List<CreatureAction> actions, double nodeBiasMaxVariancePercentage, double connectionWeightMaxVariancePercentage, double connectionWeightBound)
        {
            Dictionary<Creature, List<Connection>> availableParentConnections = parents.ToDictionary(x => x, x => x.Brain.Connections.ToList());
            Dictionary<Creature, Dictionary<int, int>> parentToOffspringNodesMap = new Dictionary<Creature, Dictionary<int, int>>();

            Creature offspring = new Creature()
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
                Creature randomParent = parents[Globals.Random.Next(parents.Count)];
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

        #region CreatureValues
        //x values map from 0 to 1.
        public double PercentMaxEnergy()
        {
            return Globals.Map(Energy, 0, MaxEnergy, 0, 1);
        }
        public double HorizontalProximityToCreature(CreatureStepInfo stepInfo)
        {
            if (stepInfo.VisibleCreaturesOrderedByDistance.Count == 0)
            {
                return 1;
            }

            //Visible creatures does not contain itself. No need to filter.
            Creature closestCreature = stepInfo.VisibleCreaturesOrderedByDistance[0];

            double horizontalDistanceToCreature = Globals.GetDistanceFrom(MX, MY, closestCreature.MX, MY);

            return Globals.Map(horizontalDistanceToCreature, 0, SightRange, 0, 1);
        }
        public double VerticalProximityToCreature(CreatureStepInfo stepInfo)
        {
            if (stepInfo.VisibleCreaturesOrderedByDistance.Count == 0)
            {
                return 1;
            }

            //Visible creatures does not contain itself. No need to filter.
            Creature closestCreature = stepInfo.VisibleCreaturesOrderedByDistance[0];

            double verticalDistanceToCreature = Globals.GetDistanceFrom(MX, MY, MX, closestCreature.MY);

            return Globals.Map(verticalDistanceToCreature, 0, SightRange, 0, 1);
        }
        public double HorizontalProximityToFood(CreatureStepInfo stepInfo)
        {
            if (stepInfo.VisibleFoodOrderedByDistance.Count == 0)
            {
                return 1;
            }

            Food closestFood = stepInfo.VisibleFoodOrderedByDistance[0];

            double horizontalDistanceToFood = Globals.GetDistanceFrom(MX, MY, closestFood.MX, MY);

            return Globals.Map(horizontalDistanceToFood, 0, SightRange, 0, 1);
        }
        public double VerticalProximityToFood(CreatureStepInfo stepInfo)
        {
            if (stepInfo.VisibleFoodOrderedByDistance.Count == 0)
            {
                return 1;
            }

            Food closestFood = stepInfo.VisibleFoodOrderedByDistance[0];

            double verticalDistanceToFood = Globals.GetDistanceFrom(MX, MY, MX, closestFood.MY);

            return Globals.Map(verticalDistanceToFood, 0, SightRange, 0, 1);
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
        private void Eat(Food food)
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

            Food closestFood = stepInfo.VisibleFoodOrderedByDistance[0];

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
                Food closestFood = stepInfo.VisibleFoodOrderedByDistance[0];

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
